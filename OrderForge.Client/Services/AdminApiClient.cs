using System.Net.Http.Json;
using System.Text.Json;
using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public interface IAdminApiClient
{
    Task<IReadOnlyList<OrganisationDto>> GetCompaniesAsync(CancellationToken cancellationToken = default);

    Task<OrganisationDto> CreateCompanyAsync(CreateCustomerCompanyRequest request, CancellationToken cancellationToken = default);

    Task<InviteUserResponse> InviteUserAsync(InviteUserRequest request, CancellationToken cancellationToken = default);
}

public sealed class AdminApiClient(HttpClient http) : IAdminApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IReadOnlyList<OrganisationDto>> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<OrganisationDto>>("api/admin/companies", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<OrganisationDto> CreateCompanyAsync(
        CreateCustomerCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/admin/companies", request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var created = await response.Content.ReadFromJsonAsync<OrganisationDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return created ?? throw new InvalidOperationException("API returned an empty body.");
    }

    public async Task<InviteUserResponse> InviteUserAsync(InviteUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/admin/users/invite", request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadFromJsonAsync<InviteUserResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return body ?? throw new InvalidOperationException("API returned an empty body.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new HttpRequestException(
            $"Admin API error {(int)response.StatusCode}: {text}",
            null,
            response.StatusCode);
    }
}
