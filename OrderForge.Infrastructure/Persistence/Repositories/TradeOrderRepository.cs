using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Orders;
using OrderForge.Domain.Orders;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public sealed class TradeOrderRepository(OrderForgeDbContext dbContext) : ITradeOrderRepository
{
    public async Task<IReadOnlyList<TradeOrderSummaryDto>> GetRecentSummariesForOrganisationAsync(
        int organisationId,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<TradeOrder>()
            .AsNoTracking()
            .Where(o => o.OrganisationId == organisationId)
            .OrderByDescending(o => o.PlacedAt)
            .Take(take)
            .Select(o => new TradeOrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.PlacedAt,
                o.Status,
                o.Total,
                o.Lines.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<TradeOrderDetailDto?> GetDetailForOrganisationAsync(
        int organisationId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<TradeOrder>()
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.OrganisationId == organisationId)
            .Select(o => new TradeOrderDetailDto(
                o.Id,
                o.OrderNumber,
                o.PlacedAt,
                o.Status,
                o.Total,
                o.Lines.Count,
                o.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new TradeOrderLineDto(l.Description, l.Quantity, l.LineTotal))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
