namespace OrderForge.Domain.Products;

public class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public decimal CostPrice { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>Keycloak user id (sub) of the creator.</summary>
    public string CreatedBy { get; set; } = string.Empty;
}
