using OrderForge.Domain.Organisations;

namespace OrderForge.Domain.Orders;

public class TradeOrder
{
    public int Id { get; set; }

    public int OrganisationId { get; set; }

    public Organisation Organisation { get; set; } = null!;

    /// <summary>Human-visible order reference (e.g. OF-10492).</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime PlacedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public ICollection<TradeOrderLine> Lines { get; set; } = [];
}
