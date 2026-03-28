namespace OrderForge.Domain.Organisations;

public class Organisation
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TradingAs { get; set; }

    /// <summary>UK Companies House number.</summary>
    public string? CompanyNumber { get; set; }

    public string? VatNumber { get; set; }

    /// <summary>Internal customer account reference.</summary>
    public string? AccountNumber { get; set; }

    /// <summary>Active, Suspended, or Inactive.</summary>
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
