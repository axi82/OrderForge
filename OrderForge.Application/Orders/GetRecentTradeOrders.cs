using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Organisations;

namespace OrderForge.Application.Orders;

public sealed record GetRecentTradeOrdersQuery(int Take = 10) : IRequest<IReadOnlyList<TradeOrderSummaryDto>>;

public sealed class GetRecentTradeOrdersQueryHandler(
    ITradeOrderRepository tradeOrders,
    IOrganisationRepository organisations,
    ICurrentUser currentUser)
    : IRequestHandler<GetRecentTradeOrdersQuery, IReadOnlyList<TradeOrderSummaryDto>>
{
    public async Task<IReadOnlyList<TradeOrderSummaryDto>> Handle(
        GetRecentTradeOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 100);
        var organisationId = await TradeOrderOrganisationContext
            .ResolveTradeOrganisationIdAsync(currentUser, organisations, cancellationToken)
            .ConfigureAwait(false);

        if (organisationId is null)
        {
            return [];
        }

        return await tradeOrders
            .GetRecentSummariesForOrganisationAsync(organisationId.Value, take, cancellationToken)
            .ConfigureAwait(false);
    }
}
