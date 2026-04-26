using System.Net.Http.Json;
using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public sealed class RouteAuthorizationAuditApi(HttpClient http) : IRouteAuthorizationAuditApi
{
    private const string Path = "api/audit/client-route-denial";

    public Task ReportClientRouteDenialAsync(ClientRouteDenialReport report, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync(Path, report, cancellationToken);
}
