namespace OrderForge.Client.Models;

public sealed class CatalogProductsListResult
{
    public List<CatalogProductItem> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? Search { get; set; }
}

public sealed class CatalogProductItem
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }
}
