using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Api.Logging;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/audit")]
public sealed class AuditController(ILogger<AuditController> logger) : ControllerBase
{
    /// <summary>
    /// Accepts a report from the Blazor client when route authorization denies an authenticated user,
    /// so the denial is visible in Seq alongside API authorization failure logs.
    /// </summary>
    [HttpPost("client-route-denial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult LogClientRouteDenial([FromBody] ClientRouteDenialRequest request)
    {
        var email = ClaimsPrincipalLogHelper.GetUserEmail(User) ?? "(unknown)";
        var userId = ClaimsPrincipalLogHelper.GetUserId(User) ?? "(anonymous)";

        logger.LogWarning(
            "Blazor route authorization denied — UserEmail: {UserEmail} — UserId: {UserId} — Path: {RelativePath} — Page: {PageType} — Required authorization: {AuthorizationSummary} — Reason: Authenticated user did not satisfy the page Authorize requirements (policy or roles mismatch).",
            email,
            userId,
            request.RelativePath,
            request.PageTypeFullName ?? "(unknown)",
            request.RouteAuthorizationSummary ?? "(unknown)");

        return NoContent();
    }
}

public sealed class ClientRouteDenialRequest
{
    public required string RelativePath { get; init; }

    public string? PageTypeFullName { get; init; }

    public string? RouteAuthorizationSummary { get; init; }
}
