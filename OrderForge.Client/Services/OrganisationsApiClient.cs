using System.Net.Http.Json;
using System.Text.Json;
using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public interface IOrganisationsApiClient
{
    Task<IReadOnlyList<OrganisationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<OrganisationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<OrganisationDto> CreateAsync(CreateOrganisationRequest request, CancellationToken cancellationToken = default);

    Task<OrganisationDto> UpdateAsync(int id, UpdateOrganisationRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class OrganisationsApiClient(HttpClient http) : IOrganisationsApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IReadOnlyList<OrganisationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<OrganisationDto>>("api/Organisations", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<OrganisationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/Organisations/{id}", cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrganisationDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<OrganisationDto> CreateAsync(
        CreateOrganisationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/Organisations", request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);
        var created = await response.Content.ReadFromJsonAsync<OrganisationDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return created ?? throw new InvalidOperationException("API returned an empty body.");
    }

    public async Task<OrganisationDto> UpdateAsync(
        int id,
        UpdateOrganisationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/Organisations/{id}", request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);
        var updated = await response.Content.ReadFromJsonAsync<OrganisationDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return updated ?? throw new InvalidOperationException("API returned an empty body.");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/Organisations/{id}", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessOrThrowAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new OrganisationsApiException(response.StatusCode, body);
    }
}

public sealed class OrganisationsApiException(System.Net.HttpStatusCode statusCode, string? responseBody)
    : Exception(BuildMessage(statusCode, responseBody))
{
    public System.Net.HttpStatusCode StatusCode { get; } = statusCode;

    public string? ResponseBody { get; } = responseBody;

    private static string BuildMessage(System.Net.HttpStatusCode statusCode, string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return $"Organisations API error: {(int)statusCode} {statusCode}";
        }

        return $"Organisations API error: {(int)statusCode} {statusCode}: {responseBody}";
    }
}
