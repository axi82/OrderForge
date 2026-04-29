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
        CollectRolesFromExistingClaims(identity, roleClaimType, roleNames);

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

        if (!string.Equals(roleClaimType, ClaimTypes.Role, StringComparison.Ordinal))
        {
            foreach (var claim in identity.FindAll(ClaimTypes.Role).ToList())
            {
                identity.RemoveClaim(claim);
            }
        }

        foreach (var roleValue in roleNames.Distinct(StringComparer.Ordinal))
        {
            identity.AddClaim(new Claim(roleClaimType, roleValue));
        }

        // ClaimsIdentity defaults RoleClaimType to ClaimTypes.Role, but Keycloak/OIDC options often
        // store roles under "roles". IsInRole / RequireRole only match RoleClaimType, so rebuild the
        // identity so role resolution sees the claims we just added.
        var roleAlignedIdentity = new ClaimsIdentity(
            identity.Claims,
            identity.AuthenticationType,
            identity.NameClaimType,
            roleClaimType);

        return new ClaimsPrincipal(roleAlignedIdentity);
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

        AppendRolesFromRolesRaw(raw, roleNames);
    }

    private static void AppendRolesFromRolesRaw(object raw, List<string> roleNames)
    {
        switch (raw)
        {
            case JsonElement el:
                if (el.ValueKind == JsonValueKind.Array)
                {
                    AppendRolesFromJsonArray(el, roleNames);
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    var s = el.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        AppendNormalizedRoleValues(s, roleNames);
                    }
                }

                break;
            case string s when !string.IsNullOrWhiteSpace(s):
                AppendNormalizedRoleValues(s, roleNames);
                break;
            case IEnumerable<string> strings:
                foreach (var r in strings)
                {
                    if (!string.IsNullOrWhiteSpace(r))
                    {
                        roleNames.Add(r);
                    }
                }

                break;
            case object[] arr:
                foreach (var o in arr)
                {
                    if (o is string r && !string.IsNullOrWhiteSpace(r))
                    {
                        roleNames.Add(r);
                    }
                }

                break;
        }
    }

    /// <summary>
    /// OIDC userinfo is often mapped onto the identity before this factory runs; those claims use
    /// <see cref="RemoteAuthenticationUserOptions.RoleClaim"/> (e.g. "roles") while
    /// <see cref="ClaimsIdentity.RoleClaimType"/> defaults to <see cref="ClaimTypes.Role"/>.
    /// </summary>
    private static void CollectRolesFromExistingClaims(
        ClaimsIdentity identity,
        string roleClaimType,
        List<string> roleNames)
    {
        foreach (var claim in identity.Claims)
        {
            if (claim.Type != roleClaimType
                && claim.Type != KeycloakRolesPropertyKey
                && claim.Type != ClaimTypes.Role)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(claim.Value))
            {
                AppendNormalizedRoleValues(claim.Value, roleNames);
            }
        }
    }

    /// <summary>Handles plain role names and JSON array strings (e.g. <c>["TradeAccount"]</c>) from OIDC mappers.</summary>
    private static void AppendNormalizedRoleValues(string value, List<string> roleNames)
    {
        var v = value.Trim();
        if (v.Length >= 2 && v[0] == '[' && v[^1] == ']')
        {
            try
            {
                using var doc = JsonDocument.Parse(v);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    AppendRolesFromJsonArray(doc.RootElement, roleNames);
                    return;
                }
            }
            catch (JsonException)
            {
                // treat as literal role name below
            }
        }

        roleNames.Add(v);
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
