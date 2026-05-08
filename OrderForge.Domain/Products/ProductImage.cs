namespace OrderForge.Domain.Products;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    /// <summary>Path within the storage zone (no leading slash), e.g. products/123/guid.jpg. Used for Bunny API and public CDN URL.</summary>
    public string StoragePath { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsMain { get; set; }

    public DateTime CreatedAt { get; set; }
}
