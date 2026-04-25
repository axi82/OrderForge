namespace OrderForge.Application.Orders;

public interface ITradeOrderRepository
{
    Task<IReadOnlyList<TradeOrderSummaryDto>> GetRecentSummariesForOrganisationAsync(
        int organisationId,
        int take,
        CancellationToken cancellationToken = default);

    Task<TradeOrderDetailDto?> GetDetailForOrganisationAsync(
        int organisationId,
        int orderId,
        CancellationToken cancellationToken = default);
}
