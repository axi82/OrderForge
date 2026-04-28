using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using OrderForge.Application.Common.Services;

namespace OrderForge.Infrastructure.Keycloak;

public sealed class KeycloakUserPasswordValidator(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakAdminOptions> adminOptions,
    IOptions<KeycloakPasswordGrantOptions> grantOptions) : IKeycloakUserPasswordValidator
{
    public async Task ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var admin = adminOptions.Value;
        var grant = grantOptions.Value;

        if (string.IsNullOrWhiteSpace(admin.BaseUrl) || string.IsNullOrWhiteSpace(admin.Realm))
        {
            throw new InvalidOperationException("Keycloak is not configured for password verification.");
        }

        if (string.IsNullOrWhiteSpace(grant.ClientId))
        {
            throw new InvalidOperationException("KeycloakPasswordGrant:ClientId is not configured.");
        }

        var tokenUrl =
            $"{admin.BaseUrl.TrimEnd('/')}/realms/{Uri.EscapeDataString(admin.Realm)}/protocol/openid-connect/token";

        var pairs = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", grant.ClientId),
            new("username", username),
            new("password", password),
            new("scope", "openid"),
        };

        if (!string.IsNullOrWhiteSpace(grant.ClientSecret))
        {
            pairs.Add(new("client_secret", grant.ClientSecret));
        }

        using var content = new FormUrlEncodedContent(pairs);
        var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = content,
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }
    }
}
