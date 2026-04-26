using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace OrderForge.Client.Services;

internal static class RouteAuthorizationFormatter
{
    public static string Describe(Type? pageType)
    {
        if (pageType is null)
        {
            return "Unknown page type";
        }

        var parts = new List<string>();
        foreach (var attr in pageType.GetCustomAttributes(inherit: true))
        {
            switch (attr)
            {
                case AuthorizeAttribute a:
                    if (!string.IsNullOrEmpty(a.Policy))
                    {
                        parts.Add($"Policy '{a.Policy}'");
                    }

                    if (!string.IsNullOrEmpty(a.Roles))
                    {
                        parts.Add($"Roles '{a.Roles}'");
                    }

                    break;
                case IAllowAnonymous:
                    parts.Add("AllowAnonymous");
                    break;
            }
        }

        return parts.Count == 0
            ? "No Authorize attribute on page type (failure may be from fallback authorization)"
            : string.Join("; ", parts);
    }
}
