using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;

namespace OrderForge.Infrastructure.Keycloak;

public sealed class KeycloakAdminService(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakAdminOptions> options,
    IMemoryCache memoryCache,
    ILogger<KeycloakAdminService> logger) : IKeycloakAdminService
{
    private const string HttpClientName = "KeycloakAdmin";
    private static readonly JsonSerializerOptions JsonCamel = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly KeycloakAdminOptions _options = options.Value;

    public async Task<CreateKeycloakUserResult> CreateUserAsync(
        CreateKeycloakUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var user = new UserRepresentation
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Enabled = request.Enabled,
            EmailVerified = request.EmailVerified
        };

        if (!string.IsNullOrEmpty(request.TemporaryPassword))
        {
            user.Credentials =
            [
                new CredentialRepresentation
                {
                    Type = "password",
                    Value = request.TemporaryPassword,
                    Temporary = true
                }
            ];
        }

        using var message = await CreateRequestAsync(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users",
            cancellationToken);
        message.Content = JsonContent.Create(user, options: JsonCamel);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new KeycloakAdminException("A user with this username or email already exists.")
            {
                StatusCode = (int)response.StatusCode
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "Create user failed.", cancellationToken).ConfigureAwait(false);
        }

        var location = response.Headers.Location?.ToString();
        var userId = ParseUserIdFromLocation(location);
        if (string.IsNullOrEmpty(userId))
        {
            userId = await FindUserIdByUsernameAsync(request.Username, cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(userId))
        {
            throw new KeycloakAdminException("User was created but the user id could not be determined.");
        }

        return new CreateKeycloakUserResult(userId, request.Username);
    }

    public async Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        var role = await GetRealmRoleByNameAsync(roleName, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            throw new KeycloakAdminException($"Realm role '{roleName}' was not found in Keycloak.");
        }

        using var message = await CreateRequestAsync(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
            cancellationToken);
        message.Content = JsonContent.Create(new[] { role }, options: JsonCamel);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "Assign realm role failed.", cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<KeycloakOrganizationResult> CreateOrganizationAsync(
        string organizationName,
        string? displayName,
        string? organizationInternetDomain,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        var domainName = ResolveOrganizationInternetDomain(organizationName, organizationInternetDomain);
        var org = new OrganizationRepresentation
        {
            Name = organizationName,
            Alias = SlugifyAlias(organizationName),
            Description = displayName,
            Domains =
            [
                new OrganizationDomainRepresentation { Name = domainName, Verified = false }
            ]
        };

        using var message = await CreateRequestAsync(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations",
            cancellationToken);
        message.Content = JsonContent.Create(org, options: JsonCamel);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new KeycloakAdminException("An organization with this name or alias already exists.")
            {
                StatusCode = (int)response.StatusCode
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "Create organization failed.", cancellationToken).ConfigureAwait(false);
        }

        var location = response.Headers.Location?.ToString();
        var id = ParseOrgIdFromLocation(location);
        if (!string.IsNullOrEmpty(id))
        {
            return new KeycloakOrganizationResult(id, organizationName);
        }

        var found = await GetOrganizationByNameAsync(organizationName, cancellationToken).ConfigureAwait(false);
        return found ?? throw new KeycloakAdminException("Organization was created but its id could not be determined.");
    }

    public async Task AddUserToOrganizationAsync(string organizationId, string userId, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        using var message = await CreateRequestAsync(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations/{Uri.EscapeDataString(organizationId)}/members",
            cancellationToken);
        // Keycloak expects a JSON-encoded string (the user id).
        message.Content = new StringContent(JsonSerializer.Serialize(userId), Encoding.UTF8, "application/json");

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "Add user to organization failed.", cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<KeycloakOrganizationResult?> GetOrganizationByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        var client = httpClientFactory.CreateClient(HttpClientName);

        var searchUrl =
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations?search={Uri.EscapeDataString(name)}";
        using var message = await CreateRequestAsync(HttpMethod.Get, searchUrl, cancellationToken);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "List organizations failed.", cancellationToken).ConfigureAwait(false);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var list = await JsonSerializer.DeserializeAsync<List<OrganizationRepresentation>>(stream, JsonCamel, cancellationToken)
            .ConfigureAwait(false);
        if (list is null || list.Count == 0)
        {
            return null;
        }

        var match = list.FirstOrDefault(o =>
            string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));
        match ??= list[0];
        if (string.IsNullOrEmpty(match.Id) || string.IsNullOrEmpty(match.Name))
        {
            return null;
        }

        return new KeycloakOrganizationResult(match.Id, match.Name);
    }

    public async Task<CreateKeycloakUserResult> InviteUserToCompanyAsync(
        InviteUserToCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = DeriveUsernameFromEmail(request.Email);
        CreateKeycloakUserResult? created = null;
        try
        {
            created = await CreateUserAsync(
                    new CreateKeycloakUserRequest(
                        username,
                        request.Email,
                        request.FirstName,
                        request.LastName,
                        Enabled: true,
                        request.TemporaryPassword),
                    cancellationToken)
                .ConfigureAwait(false);

            await AssignRealmRoleAsync(created.UserId, request.RealmRoleName, cancellationToken).ConfigureAwait(false);
            await AddUserToOrganizationAsync(request.KeycloakOrganizationId, created.UserId, cancellationToken)
                .ConfigureAwait(false);
            return created;
        }
        catch
        {
            if (created is not null)
            {
                try
                {
                    await DeleteUserAsync(created.UserId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception deleteEx)
                {
                    logger.LogWarning(deleteEx, "Failed to roll back Keycloak user {UserId} after invite error", created.UserId);
                }
            }

            throw;
        }
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        using var message = await CreateRequestAsync(
            HttpMethod.Delete,
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users/{Uri.EscapeDataString(userId)}",
            cancellationToken);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "Delete user failed.", cancellationToken).ConfigureAwait(false);
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("KeycloakAdmin:BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException(
                "KeycloakAdmin:ClientSecret is not configured. Set user secrets or environment KeycloakAdmin__ClientSecret.");
        }
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method,
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var cacheKey = $"kcadmin-token:{_options.Realm}:{_options.ClientId}";
        if (memoryCache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"realms/{Uri.EscapeDataString(_options.Realm)}/protocol/openid-connect/token");
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret)
        ]);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            logger.LogWarning(
                "Keycloak client credentials failed for realm {Realm}, client {ClientId}: HTTP {Status}. Body: {Body}",
                _options.Realm,
                _options.ClientId,
                (int)response.StatusCode,
                errorBody);
            throw new KeycloakAdminException(
                $"Keycloak client credentials token request failed. HTTP {(int)response.StatusCode}")
            {
                StatusCode = (int)response.StatusCode,
                ResponseBody = errorBody
            };
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var tokenResponse =
            await JsonSerializer.DeserializeAsync<TokenResponse>(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
        {
            throw new KeycloakAdminException("Keycloak token response did not contain an access_token.");
        }

        var ttl = TimeSpan.FromSeconds(Math.Max(60, tokenResponse.ExpiresIn - 60));
        memoryCache.Set(cacheKey, tokenResponse.AccessToken, ttl);
        return tokenResponse.AccessToken;
    }

    private async Task<RoleRepresentation?> GetRealmRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url =
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/roles/{Uri.EscapeDataString(roleName)}";
        using var message = await CreateRequestAsync(HttpMethod.Get, url, cancellationToken);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowKeycloakErrorAsync(response, "Get realm role failed.", cancellationToken).ConfigureAwait(false);
        }

        return await response.Content.ReadFromJsonAsync<RoleRepresentation>(JsonCamel, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<string?> FindUserIdByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var url =
            $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users?username={Uri.EscapeDataString(username)}&exact=true";
        using var message = await CreateRequestAsync(HttpMethod.Get, url, cancellationToken);
        using var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var users = await response.Content.ReadFromJsonAsync<List<UserRepresentation>>(JsonCamel, cancellationToken)
            .ConfigureAwait(false);
        return users?.FirstOrDefault()?.Id;
    }

    private static async Task ThrowKeycloakErrorAsync(
        HttpResponseMessage response,
        string message,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new KeycloakAdminException($"{message} HTTP {(int)response.StatusCode}")
        {
            StatusCode = (int)response.StatusCode,
            ResponseBody = body
        };
    }

    private static string? ParseUserIdFromLocation(string? location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return null;
        }

        var parts = location.TrimEnd('/').Split('/');
        return parts.Length > 0 ? parts[^1] : null;
    }

    private static string? ParseOrgIdFromLocation(string? location) => ParseUserIdFromLocation(location);

    private static string DeriveUsernameFromEmail(string email)
    {
        var at = email.IndexOf('@', StringComparison.Ordinal);
        var local = at > 0 ? email[..at] : email;
        var sanitized = new string(local.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "user" : sanitized.ToLowerInvariant();
    }

    private static string SlugifyAlias(string name)
    {
        var chars = name.Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-').ToArray();
        var s = new string(chars).Trim('-');
        while (s.Contains("--", StringComparison.Ordinal))
        {
            s = s.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrEmpty(s) ? "org" : s;
    }

    /// <summary>Keycloak requires at least one organization domain; use a reserved-style placeholder if none supplied.</summary>
    private static string ResolveOrganizationInternetDomain(string organizationName, string? organizationInternetDomain)
    {
        if (!string.IsNullOrWhiteSpace(organizationInternetDomain))
        {
            return organizationInternetDomain.Trim().ToLowerInvariant();
        }

        return $"{SlugifyAlias(organizationName)}.orderforge.test";
    }

    private sealed class UserRepresentation
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? Enabled { get; set; }
        public bool? EmailVerified { get; set; }
        public List<CredentialRepresentation>? Credentials { get; set; }
    }

    private sealed class CredentialRepresentation
    {
        public string? Type { get; set; }
        public string? Value { get; set; }
        public bool? Temporary { get; set; }
    }

    private sealed class RoleRepresentation
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class OrganizationRepresentation
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? Description { get; set; }

        public List<OrganizationDomainRepresentation>? Domains { get; set; }
    }

    private sealed class OrganizationDomainRepresentation
    {
        public string? Name { get; set; }
        public bool Verified { get; set; }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
