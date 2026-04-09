namespace OrderForge.Application.Admin;

public sealed record ProductDto(
    int Id,
    string Sku,
    string Name,
    string? ShortDescription,
    string? Description,
    string? Brand,
    decimal CostPrice,
    decimal BasePrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy);
