using OrderForge.Application.Common;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Products;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> ExistsWithSkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithProductCodeAsync(string productCode, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    /// <summary>Active products only; same search and sort semantics as <see cref="GetPagedAsync"/>.</summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetActivePagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);
}
