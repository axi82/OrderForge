using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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

            default:
                return false;
        }
    }
}
