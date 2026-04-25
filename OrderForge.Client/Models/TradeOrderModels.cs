namespace OrderForge.Client.Models;

public sealed class TradeOrderSummaryModel
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime PlacedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public int ItemCount { get; set; }
}

public sealed class TradeOrderLineModel
{
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal LineTotal { get; set; }
}

public sealed class TradeOrderDetailModel
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime PlacedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public int ItemCount { get; set; }

    public List<TradeOrderLineModel> Lines { get; set; } = [];
}
