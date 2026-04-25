namespace OrderForge.Application.Orders;

public sealed record TradeOrderSummaryDto(
    int Id,
    string OrderNumber,
    DateTime PlacedAt,
    string Status,
    decimal Total,
    int ItemCount);

public sealed record TradeOrderLineDto(string Description, decimal Quantity, decimal LineTotal);

public sealed record TradeOrderDetailDto(
    int Id,
    string OrderNumber,
    DateTime PlacedAt,
    string Status,
    decimal Total,
    int ItemCount,
    IReadOnlyList<TradeOrderLineDto> Lines);
