using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderForge.Application.Orders;
using OrderForge.Api;

namespace OrderForge.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TradeAccount)]
[Route("api/trade/orders")]
public sealed class TradeOrdersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TradeOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TradeOrderSummaryDto>>> GetRecent(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var list = await sender.Send(new GetRecentTradeOrdersQuery(take), cancellationToken);
        return Ok(list);
    }

    [HttpGet("{orderId:int}")]
    [ProducesResponseType(typeof(TradeOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TradeOrderDetailDto>> GetById(int orderId, CancellationToken cancellationToken)
    {
        var dto = await sender.Send(new GetTradeOrderByIdQuery(orderId), cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
