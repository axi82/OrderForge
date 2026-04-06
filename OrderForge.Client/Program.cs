using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrderForge.Client;
using OrderForge.Client.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUri = builder.GetOrderForgeApiBaseUri();

builder.Services
    .AddOrderForgeAuthorizationPolicies()
    .AddOrderForgeOidcAuthentication(builder.Configuration)
    .AddOrderForgeApiClients(apiUri);

await builder.Build().RunAsync();
