using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace OrderForge.Api.Logging;

/// <summary>
/// Logs authorization outcomes (challenge vs forbid) and failed requirements, then delegates to the default handler.
/// </summary>
public sealed class AuthorizationFailureLoggingMiddlewareResultHandler(ILogger<AuthorizationFailureLoggingMiddlewareResultHandler> logger)
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy? policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded)
        {
            var endpoint = context.GetEndpoint();
            var route = context.Request.Path.Value ?? string.Empty;
            var resource = string.IsNullOrEmpty(endpoint?.DisplayName) ? route : $"{endpoint.DisplayName} ({route})";

            string outcome;
            if (authorizeResult.Challenged)
            {
                outcome = "Challenged (typically 401)";
            }
            else if (authorizeResult.Forbidden)
            {
                outcome = "Forbidden (403)";
            }
            else
            {
                outcome = "Not authorized";
            }

            var policySummary = policy is null ? "(no policy object)" : SummarizePolicy(policy);
            var failureDetail = FormatFailure(authorizeResult.AuthorizationFailure);
            var userEmail = ClaimsPrincipalLogHelper.GetUserEmail(context.User) ?? "(unknown)";
            var userId = ClaimsPrincipalLogHelper.GetUserId(context.User) ?? "(anonymous)";

            logger.LogWarning(
                "API authorization denied — Outcome: {AuthorizationOutcome} — UserEmail: {UserEmail} — UserId: {UserId} — HTTP {HttpMethod} {Resource} — Policy requirements: {PolicySummary} — Reason: {FailureReason}",
                outcome,
                userEmail,
                userId,
                context.Request.Method,
                resource,
                policySummary,
                failureDetail);
        }

        await _defaultHandler.HandleAsync(next, context, policy!, authorizeResult).ConfigureAwait(false);
    }

    private static string SummarizePolicy(AuthorizationPolicy policy)
    {
        if (policy.Requirements.Count == 0)
        {
            return "(none)";
        }

        var sb = new StringBuilder();
        for (var i = 0; i < policy.Requirements.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(DescribeRequirement(policy.Requirements[i]));
        }

        return sb.ToString();
    }

    private static string FormatFailure(AuthorizationFailure? failure)
    {
        if (failure is null)
        {
            return "Failed requirements: (no AuthorizationFailure — e.g. unauthenticated user)";
        }

        var failed = failure.FailedRequirements.ToList();
        if (failed.Count == 0)
        {
            return "Failed requirements: (none listed)";
        }

        return "Failed requirements: " + string.Join("; ", failed.Select(DescribeRequirement));
    }

    private static string DescribeRequirement(IAuthorizationRequirement requirement) =>
        requirement switch
        {
            RolesAuthorizationRequirement roles =>
                $"RolesAuthorizationRequirement[{string.Join(", ", roles.AllowedRoles)}]",
            ClaimsAuthorizationRequirement claims =>
                $"ClaimsAuthorizationRequirement({claims.ClaimType})",
            DenyAnonymousAuthorizationRequirement => "DenyAnonymousAuthorizationRequirement",
            AssertionRequirement => "AssertionRequirement",
            OperationAuthorizationRequirement op => $"OperationAuthorizationRequirement({op.Name})",
            NameAuthorizationRequirement name => $"NameAuthorizationRequirement({name.RequiredName})",
            _ => requirement.GetType().Name
        };
}
