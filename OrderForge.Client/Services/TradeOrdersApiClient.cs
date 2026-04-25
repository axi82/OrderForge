using System.Net.Http.Json;
using System.Text.Json;
using OrderForge.Client.Models;

namespace OrderForge.Client.Services;

public interface ITradeOrdersApiClient
{
    Task<IReadOnlyList<TradeOrderSummaryModel>> GetRecentAsync(int take = 10, CancellationToken cancellationToken = default);

    Task<TradeOrderDetailModel?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
}

public sealed class TradeOrdersApiClient(HttpClient http) : ITradeOrdersApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<IReadOnlyList<TradeOrderSummaryModel>> GetRecentAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await http
            .GetAsync($"api/trade/orders?take={take}", cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var list = await response.Content
            .ReadFromJsonAsync<List<TradeOrderSummaryModel>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<TradeOrderDetailModel?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/trade/orders/{orderId}", cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content
            .ReadFromJsonAsync<TradeOrderDetailModel>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
