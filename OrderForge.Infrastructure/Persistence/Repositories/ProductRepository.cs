using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Products;
using OrderForge.Domain.Products;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(OrderForgeDbContext dbContext)
    : EfRepository<Product>(dbContext), IProductRepository
{
    public async Task<bool> ExistsWithSkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim();
        return await DbContext
            .Set<Product>()
            .AsNoTracking()
            .AnyAsync(p => p.Sku.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<Product>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Sku.Contains(term)
                || p.Name.Contains(term)
                || (p.Brand != null && p.Brand.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
