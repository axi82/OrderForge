using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace OrderForge.Client.Services;

/// <summary>
/// Attaches the access token to the configured API base URL. The stock <see cref="BaseAddressAuthorizationMessageHandler"/>
/// only matches <see cref="NavigationManager.BaseUri"/> (the WASM app), not a separate API origin.
/// </summary>
public sealed class OrderForgeApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public OrderForgeApiAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        NavigationManager navigationManager,
        Uri apiBase)
        : base(provider, navigationManager)
    {
        ConfigureHandler(new[] { apiBase.AbsoluteUri });
    }
}
