using System.Security.Claims;
using System.Text.Json;

namespace OrderForge.Api.Security;

/// <summary>
/// Flattens Keycloak <c>realm_access.roles</c> into <see cref="ClaimTypes.Role"/> and normalizes organization id into <c>keycloak_org_id</c>.
/// </summary>
public static class KeycloakJwtClaimsMapper
{
    public const string KeycloakOrganizationIdClaim = "keycloak_org_id";

    public static void Map(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        MapRealmRoles(principal, identity);
        MapOrganization(principal, identity);
    }

    private static void MapRealmRoles(ClaimsPrincipal principal, ClaimsIdentity identity)
    {
        var raw = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (!doc.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var role in roles.EnumerateArray())
            {
                var name = role.GetString();
                if (!string.IsNullOrEmpty(name))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, name));
                }
            }
        }
        catch (JsonException)
        {
            // ignore malformed token fragment
        }
    }

    private static void MapOrganization(ClaimsPrincipal principal, ClaimsIdentity identity)
    {
        if (identity.HasClaim(c => c.Type == KeycloakOrganizationIdClaim))
        {
            return;
        }

        var raw = principal.FindFirst("organization")?.Value;
        var id = ParseOrganizationId(raw);
        if (!string.IsNullOrEmpty(id))
        {
            identity.AddClaim(new Claim(KeycloakOrganizationIdClaim, id));
        }
    }

    private static string? ParseOrganizationId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.StartsWith('['))
        {
            try
            {
                var ids = JsonSerializer.Deserialize<List<string>>(trimmed);
                return ids?.FirstOrDefault(static s => !string.IsNullOrEmpty(s));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        if (trimmed.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                if (doc.RootElement.TryGetProperty("id", out var idEl))
                {
                    return idEl.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }
        }

        return trimmed;
    }
}
