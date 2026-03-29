using Serilog.Context;

namespace OrderForge.Api.Logging;

/// <summary>
/// Adds <see cref="CorrelationId"/> to Serilog's log context for the request (header <c>X-Correlation-Id</c> or trace identifier).
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Response.Headers.Append(HeaderName, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var header = context.Request.Headers[HeaderName];
        if (header.Count > 0 && !string.IsNullOrWhiteSpace(header[0]))
        {
            return header[0]!;
        }

        return context.TraceIdentifier;
    }
}
