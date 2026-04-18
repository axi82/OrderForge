using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace OrderForge.Client.Services;

public sealed class AppNavigator
{
    private readonly NavigationManager _navigation;

    public AppNavigator(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    public void GoToSignInPage(string returnUrl) =>
        _navigation.NavigateTo(Routes.Auth.OidcLoginWithReturnUrl(returnUrl));

    public void BeginOidcSignIn(string returnUrl) =>
        _navigation.NavigateTo(Routes.Auth.OidcLoginWithReturnUrl(returnUrl));

    public void SignOut() =>
        _navigation.NavigateToLogout(Routes.Auth.OidcLogout);
}
