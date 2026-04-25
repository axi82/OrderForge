namespace OrderForge.Domain.Orders;

public class TradeOrderLine
{
    public int Id { get; set; }

    public int TradeOrderId { get; set; }

    public TradeOrder TradeOrder { get; set; } = null!;

    public int SortOrder { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
