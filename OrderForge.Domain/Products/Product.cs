namespace OrderForge.Domain.Products;

public class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    /// <summary>External / ERP product code (e.g. stock system "Product Code").</summary>
    public string ProductCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public decimal CostPrice { get; set; }

    public decimal BasePrice { get; set; }

    /// <summary>Commodity or tariff code text from source systems (CSV "Com. Code Description").</summary>
    public string? CommodityCodeDescription { get; set; }

    public string? SupplierAccountCode { get; set; }

    public string? PartNumber { get; set; }

    public decimal QuantityInStock { get; set; }

    public decimal QuantityAllocated { get; set; }

    public decimal QuantityOnOrder { get; set; }

    public decimal FreeStock { get; set; }

    public string? Barcode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>Keycloak user id (sub) of the creator.</summary>
    public string CreatedBy { get; set; } = string.Empty;
}
