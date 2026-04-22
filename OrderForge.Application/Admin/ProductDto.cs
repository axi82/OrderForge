namespace OrderForge.Application.Admin;

public sealed record ProductDto(
    int Id,
    string Sku,
    string ProductCode,
    string Name,
    string? ShortDescription,
    string? Description,
    string? Brand,
    string? CommodityCodeDescription,
    string? SupplierAccountCode,
    string? PartNumber,
    decimal QuantityInStock,
    decimal QuantityAllocated,
    decimal QuantityOnOrder,
    decimal FreeStock,
    string? Barcode,
    decimal CostPrice,
    decimal BasePrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy);
