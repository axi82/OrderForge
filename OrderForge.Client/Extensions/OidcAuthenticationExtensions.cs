using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderForge.Client.Authentication;
using OrderForge.Client;

namespace OrderForge.Client.Extensions;

public static class OidcAuthenticationExtensions
{
    public static IServiceCollection AddOrderForgeOidcAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOidcAuthentication(options =>
        {
            configuration.Bind("Oidc", options.ProviderOptions);
            options.ProviderOptions.ResponseType = "code";
            options.AuthenticationPaths.LogInFailedPath = Routes.Auth.LoginFailed;
            options.AuthenticationPaths.LogOutSucceededPath = string.Empty;
            options.UserOptions.RoleClaim = "roles";

            var authority = options.ProviderOptions.Authority?.TrimEnd('/');
            if (!string.IsNullOrEmpty(authority))
            {
                options.ProviderOptions.Authority = authority;
            }

            foreach (var scope in new[] { "profile", "email" })
            {
                if (!options.ProviderOptions.DefaultScopes.Contains(scope))
                {
                    options.ProviderOptions.DefaultScopes.Add(scope);
                }
            }
        }).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, KeycloakUserClaimFactory>();

        return services;
    }
}
