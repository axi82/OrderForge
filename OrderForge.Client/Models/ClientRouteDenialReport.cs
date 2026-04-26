namespace OrderForge.Client.Models;

public sealed class ClientRouteDenialReport
{
    public required string RelativePath { get; init; }

    public string? PageTypeFullName { get; init; }

    public string? RouteAuthorizationSummary { get; init; }
}
