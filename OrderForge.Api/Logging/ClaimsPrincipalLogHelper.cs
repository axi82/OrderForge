using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrderForge.Api.Logging;

internal static class ClaimsPrincipalLogHelper
{
    private static readonly string[] EmailClaimTypes =
    [
        JwtRegisteredClaimNames.Email,
        ClaimTypes.Email,
        "email",
        "preferred_username",
    ];

    public static string? GetUserEmail(ClaimsPrincipal user)
    {
        if (user.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        foreach (var claimType in EmailClaimTypes)
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return user.Identity.Name;
    }

    public static string? GetUserId(ClaimsPrincipal user)
    {
        if (user.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        return user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
