using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public interface IRouteAuthorizationAuditApi
{
    Task ReportClientRouteDenialAsync(ClientRouteDenialReport report, CancellationToken cancellationToken = default);
}
