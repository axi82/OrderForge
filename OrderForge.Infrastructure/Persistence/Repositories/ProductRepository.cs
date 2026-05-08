using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Products;
using OrderForge.Domain.Products;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(OrderForgeDbContext dbContext)
    : EfRepository<Product>(dbContext), IProductRepository
{
    public async Task<Product?> GetByIdWithImagesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Product>()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsWithSkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim();
        return await DbContext
            .Set<Product>()
            .AsNoTracking()
            .AnyAsync(p => p.Sku.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsWithProductCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        var normalized = productCode.Trim();
        return await DbContext
            .Set<Product>()
            .AsNoTracking()
            .AnyAsync(p => p.ProductCode.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default) =>
        GetFilteredPagedAsync(activeOnly: false, page, pageSize, search, cancellationToken);

    public Task<(IReadOnlyList<Product> Items, int TotalCount)> GetActivePagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default) =>
        GetFilteredPagedAsync(activeOnly: true, page, pageSize, search, cancellationToken);

    private async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredPagedAsync(
        bool activeOnly,
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = DbContext.Set<Product>().AsNoTracking().AsQueryable();

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Sku.Contains(term)
                || p.ProductCode.Contains(term)
                || p.Name.Contains(term)
                || (p.Brand != null && p.Brand.Contains(term))
                || (p.PartNumber != null && p.PartNumber.Contains(term))
                || (p.Barcode != null && p.Barcode.Contains(term))
                || (p.SupplierAccountCode != null && p.SupplierAccountCode.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(p => p.Images)
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
