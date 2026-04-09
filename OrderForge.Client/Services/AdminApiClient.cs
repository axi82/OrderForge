using System.Net.Http.Json;
using System.Text.Json;
using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public interface IAdminApiClient
{
    Task<IReadOnlyList<OrganisationDto>> GetCompaniesAsync(CancellationToken cancellationToken = default);

    Task<OrganisationDto> CreateCompanyAsync(CreateCustomerCompanyRequest request, CancellationToken cancellationToken = default);

    Task<InviteUserResponse> InviteUserAsync(InviteUserRequest request, CancellationToken cancellationToken = default);

    Task<AdminUsersListResult> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<AdminProductsListResult> GetProductsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default);
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

    public async Task<AdminUsersListResult> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var qs = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            qs += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        var result = await http
            .GetFromJsonAsync<AdminUsersListResult>($"api/admin/users?{qs}", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return result ?? new AdminUsersListResult();
    }

    public async Task<AdminProductsListResult> GetProductsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var qs = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            qs += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        var result = await http
            .GetFromJsonAsync<AdminProductsListResult>($"api/admin/products?{qs}", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return result ?? new AdminProductsListResult();
    }

    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/admin/products", request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return created ?? throw new InvalidOperationException("API returned an empty body.");
    }

    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/admin/products/{productId}", cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
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
