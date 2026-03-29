using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Serilog.Context;

namespace OrderForge.Api.Logging;

/// <summary>
/// Adds <see cref="UserId"/> (JWT <c>sub</c> or name identifier) to Serilog's log context when the user is authenticated.
/// </summary>
public sealed class UserLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = ResolveUserId(context.User);
        if (string.IsNullOrEmpty(userId))
        {
            await next(context);
            return;
        }

        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context);
        }
    }

    private static string? ResolveUserId(ClaimsPrincipal user)
    {
        if (user.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        return user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
