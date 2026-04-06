using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace OrderForge.Client.Authentication;

/// <summary>
/// Expands Keycloak&apos;s <c>roles</c> JSON array on the OIDC user into one claim per role,
/// using <see cref="RemoteAuthenticationUserOptions.RoleClaim"/> as the claim type (defaults to <see cref="ClaimTypes.Role"/> when unset).
/// </summary>
public sealed class KeycloakUserClaimFactory(IAccessTokenProviderAccessor accessor)
    : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    private const string KeycloakRolesPropertyKey = "roles";

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

        if (!account.AdditionalProperties.TryGetValue(KeycloakRolesPropertyKey, out var rolesValue) ||
            rolesValue is not JsonElement rolesElement ||
            rolesElement.ValueKind != JsonValueKind.Array)
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

        foreach (var role in rolesElement.EnumerateArray())
        {
            var roleValue = role.GetString();
            if (!string.IsNullOrWhiteSpace(roleValue))
            {
                identity.AddClaim(new Claim(roleClaimType, roleValue));
            }
        }

        return user;
    }
}
