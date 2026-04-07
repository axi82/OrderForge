using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Common;

namespace OrderForge.Api.ExceptionHandling;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException vex:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                var details = new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                };
                foreach (var group in vex.Errors.GroupBy(e => e.PropertyName))
                {
                    details.Errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
                }

                await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
                return true;

            case KeyNotFoundException knf:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Not found",
                        Detail = knf.Message,
                        Status = StatusCodes.Status404NotFound
                    },
                    cancellationToken);
                return true;

            case UnauthorizedAccessException uax:
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Forbidden",
                        Detail = uax.Message,
                        Status = StatusCodes.Status403Forbidden
                    },
                    cancellationToken);
                return true;

            case KeycloakAdminException kax:
                var statusCode = kax.StatusCode is >= 400 and < 600
                    ? kax.StatusCode.Value
                    : StatusCodes.Status502BadGateway;
                httpContext.Response.StatusCode = statusCode;
                var kcDetail = string.IsNullOrWhiteSpace(kax.ResponseBody)
                    ? kax.Message
                    : $"{kax.Message}. Keycloak: {kax.ResponseBody}";
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Identity provider error",
                        Detail = kcDetail,
                        Status = statusCode
                    },
                    cancellationToken);
                return true;

            case InvalidOperationException iox:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Request not valid",
                        Detail = iox.Message,
                        Status = StatusCodes.Status400BadRequest
                    },
                    cancellationToken);
                return true;

            default:
                return false;
        }
    }
}
