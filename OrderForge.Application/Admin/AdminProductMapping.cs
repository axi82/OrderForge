using OrderForge.Application.Storage;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Admin;

public static class AdminProductMapping
{
    public static ProductDto ToProductDto(this Product product, IBunnyObjectStorage bunnyObjectStorage)
    {
        var images = product
            .Images.OrderBy(i => i.SortOrder)
            .Select(i => new ProductImageDto(
                i.Id,
                bunnyObjectStorage.GetPublicUrl(i.StoragePath),
                i.SortOrder,
                i.IsMain))
            .ToList();

        return new ProductDto(
            product.Id,
            product.Sku,
            product.ProductCode,
            product.Name,
            product.ShortDescription,
            product.Description,
            product.Brand,
            product.CommodityCodeDescription,
            product.SupplierAccountCode,
            product.PartNumber,
            product.QuantityInStock,
            product.QuantityAllocated,
            product.QuantityOnOrder,
            product.FreeStock,
            product.Barcode,
            product.CostPrice,
            product.BasePrice,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt,
            product.CreatedBy,
            images);
    }
}
