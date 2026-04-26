using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace OrderForge.Client.Authentication;

/// <summary>
/// Maps Keycloak realm roles onto the configured role claim type.
/// Supports a top-level <c>roles</c> array and <c>realm_access.roles</c> (object or JSON string),
/// matching Keycloak <c>realm_access.roles</c> on access tokens and userinfo.
/// </summary>
public sealed class KeycloakUserClaimFactory(IAccessTokenProviderAccessor accessor)
    : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    private const string KeycloakRolesPropertyKey = "roles";

    private const string RealmAccessPropertyKey = "realm_access";

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);
        var identity = user.Identity as ClaimsIdentity;

        if (account is null || identity is null)
        {
            return user;
        }

        var roleClaimType = string.IsNullOrEmpty(options.RoleClaim) ? ClaimTypes.Role : options.RoleClaim;

        var roleNames = new List<string>();
        CollectRolesFromAdditionalProperty(account, KeycloakRolesPropertyKey, roleNames);
        CollectRolesFromAdditionalProperty(account, RealmAccessPropertyKey, roleNames);

        if (roleNames.Count == 0)
        {
            return user;
        }

        foreach (var claim in identity.FindAll(KeycloakRolesPropertyKey).ToList())
        {
            identity.RemoveClaim(claim);
        }

        if (!string.Equals(roleClaimType, KeycloakRolesPropertyKey, StringComparison.Ordinal))
        {
            foreach (var claim in identity.FindAll(roleClaimType).ToList())
            {
                identity.RemoveClaim(claim);
            }
        }

        foreach (var roleValue in roleNames.Distinct(StringComparer.Ordinal))
        {
            identity.AddClaim(new Claim(roleClaimType, roleValue));
        }

        return user;
    }

    private static void CollectRolesFromAdditionalProperty(
        RemoteUserAccount account,
        string key,
        List<string> roleNames)
    {
        if (!account.AdditionalProperties.TryGetValue(key, out var raw) || raw is null)
        {
            return;
        }

        if (key == RealmAccessPropertyKey)
        {
            AppendRolesFromRealmAccess(raw, roleNames);
            return;
        }

        if (raw is JsonElement el && el.ValueKind == JsonValueKind.Array)
        {
            AppendRolesFromJsonArray(el, roleNames);
        }
    }

    private static void AppendRolesFromRealmAccess(object raw, List<string> roleNames)
    {
        if (raw is JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.String)
            {
                var text = el.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    TryAppendRolesFromRealmAccessJson(text, roleNames);
                }

                return;
            }

            AppendRolesFromRealmAccessObject(el, roleNames);
        }
        else if (raw is string s && !string.IsNullOrWhiteSpace(s))
        {
            TryAppendRolesFromRealmAccessJson(s, roleNames);
        }
    }

    private static void TryAppendRolesFromRealmAccessJson(string json, List<string> roleNames)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            AppendRolesFromRealmAccessObject(doc.RootElement, roleNames);
        }
        catch (JsonException)
        {
            // ignore malformed fragment
        }
    }

    private static void AppendRolesFromRealmAccessObject(JsonElement root, List<string> roleNames)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("roles", out var roles) ||
            roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        AppendRolesFromJsonArray(roles, roleNames);
    }

    private static void AppendRolesFromJsonArray(JsonElement array, List<string> roleNames)
    {
        foreach (var role in array.EnumerateArray())
        {
            var roleValue = role.GetString();
            if (!string.IsNullOrWhiteSpace(roleValue))
            {
                roleNames.Add(roleValue);
            }
        }
    }
}
