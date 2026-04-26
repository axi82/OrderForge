using Serilog.Context;

namespace OrderForge.Api.Logging;

/// <summary>
/// Adds <c>UserId</c> (JWT <c>sub</c>) and <c>UserEmail</c> to Serilog's log context when available for Seq troubleshooting.
/// </summary>
public sealed class UserLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = ClaimsPrincipalLogHelper.GetUserId(context.User);
        var email = ClaimsPrincipalLogHelper.GetUserEmail(context.User);

        if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(email))
        {
            await next(context);
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            using (LogContext.PushProperty("UserId", userId ?? string.Empty))
            {
                await next(context);
            }

            return;
        }

        if (string.IsNullOrEmpty(userId))
        {
            using (LogContext.PushProperty("UserEmail", email))
            {
                await next(context);
            }

            return;
        }

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserEmail", email))
        {
            await next(context);
        }
    }
}
