using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Organisations;

namespace OrderForge.Application.Orders;

public sealed record GetTradeOrderByIdQuery(int OrderId) : IRequest<TradeOrderDetailDto?>;

public sealed class GetTradeOrderByIdQueryHandler(
    ITradeOrderRepository tradeOrders,
    IOrganisationRepository organisations,
    ICurrentUser currentUser)
    : IRequestHandler<GetTradeOrderByIdQuery, TradeOrderDetailDto?>
{
    public async Task<TradeOrderDetailDto?> Handle(
        GetTradeOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var organisationId = await TradeOrderOrganisationContext
            .ResolveTradeOrganisationIdAsync(currentUser, organisations, cancellationToken)
            .ConfigureAwait(false);

        if (organisationId is null)
        {
            return null;
        }

        return await tradeOrders
            .GetDetailForOrganisationAsync(organisationId.Value, request.OrderId, cancellationToken)
            .ConfigureAwait(false);
    }
}
