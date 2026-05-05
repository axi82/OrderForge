using System.Net.Http.Json;
using System.Text.Json;
using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public interface ICatalogApiClient
{
    Task<CatalogProductsListResult> GetProductsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);
}

public sealed class CatalogApiClient(HttpClient http) : ICatalogApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<CatalogProductsListResult> GetProductsAsync(
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

        var response = await http
            .GetAsync($"api/catalog/products?{qs}", cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Catalog API returned {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        var result = await response.Content
            .ReadFromJsonAsync<CatalogProductsListResult>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? new CatalogProductsListResult();
    }
}
