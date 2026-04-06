using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using OrderForge.Client.Services;

namespace OrderForge.Client.Extensions;

public static class ApiHttpClientExtensions
{
    public static IServiceCollection AddOrderForgeApiClients(this IServiceCollection services, Uri apiBaseUri)
    {
        services.AddTransient<OrderForgeApiAuthorizationMessageHandler>(sp =>
            new OrderForgeApiAuthorizationMessageHandler(
                sp.GetRequiredService<IAccessTokenProvider>(),
                sp.GetRequiredService<NavigationManager>(),
                apiBaseUri));

        services.AddHttpClient<IOrganisationsApiClient, OrganisationsApiClient>(client => client.BaseAddress = apiBaseUri)
            .AddHttpMessageHandler<OrderForgeApiAuthorizationMessageHandler>();

        services.AddHttpClient<IAdminApiClient, AdminApiClient>(client => client.BaseAddress = apiBaseUri)
            .AddHttpMessageHandler<OrderForgeApiAuthorizationMessageHandler>();

        return services;
    }
}
