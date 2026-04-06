using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrderForge.Client;
using OrderForge.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    apiBase = builder.HostEnvironment.BaseAddress;
}

var apiUri = new Uri(apiBase.TrimEnd('/') + "/", UriKind.Absolute);

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.SupplierAdmin,
        p => p.RequireRole("SupplierAdmin"));
    options.AddPolicy(
        AuthorizationPolicies.SupplierStaff,
        p => p.RequireRole("SupplierAdmin", "SupplierViewer"));
    options.AddPolicy(
        AuthorizationPolicies.InviteUsers,
        p => p.RequireRole("SupplierAdmin", "CompanyAdmin"));
});
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.AuthenticationPaths.LogInFailedPath = "authentication/login-failed";
    options.UserOptions.RoleClaim = "roles";

    var authority = options.ProviderOptions.Authority?.TrimEnd('/');
    if (!string.IsNullOrEmpty(authority))
    {
        options.ProviderOptions.Authority = authority;
    }

    if (!options.ProviderOptions.DefaultScopes.Contains("profile"))
    {
        options.ProviderOptions.DefaultScopes.Add("profile");
    }

    if (!options.ProviderOptions.DefaultScopes.Contains("email"))
    {
        options.ProviderOptions.DefaultScopes.Add("email");
    }
});

builder.Services.AddTransient<OrderForgeApiAuthorizationMessageHandler>(sp =>
    new OrderForgeApiAuthorizationMessageHandler(
        sp.GetRequiredService<IAccessTokenProvider>(),
        sp.GetRequiredService<NavigationManager>(),
        apiUri));

builder.Services.AddHttpClient<IOrganisationsApiClient, OrganisationsApiClient>(client => client.BaseAddress = apiUri)
    .AddHttpMessageHandler<OrderForgeApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient<IAdminApiClient, AdminApiClient>(client => client.BaseAddress = apiUri)
    .AddHttpMessageHandler<OrderForgeApiAuthorizationMessageHandler>();

await builder.Build().RunAsync();
