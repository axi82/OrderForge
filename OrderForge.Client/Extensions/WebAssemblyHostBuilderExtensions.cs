using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace OrderForge.Client.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    /// <summary>
    /// Resolves the API base URL from configuration (<c>ApiBaseUrl</c>) or falls back to the host base address.
    /// </summary>
    public static Uri GetOrderForgeApiBaseUri(this WebAssemblyHostBuilder builder)
    {
        var apiBase = builder.Configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(apiBase))
        {
            apiBase = builder.HostEnvironment.BaseAddress;
        }

        return new Uri(apiBase.TrimEnd('/') + "/", UriKind.Absolute);
    }
}
