using FluentValidation;
using MediatR;
using OrderForge.Application.Products;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Catalog;

public sealed record CatalogProductDto(int Id, string Sku, string Name, string? ShortDescription);

public sealed record CatalogProductsListResponse(
    IReadOnlyList<CatalogProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search);

public sealed record GetCatalogProductsQuery(int Page, int PageSize, string? Search)
    : IRequest<CatalogProductsListResponse>;

public sealed class GetCatalogProductsQueryValidator : AbstractValidator<GetCatalogProductsQuery>
{
    public GetCatalogProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200).When(x => x.Search is not null);
    }
}

public sealed class GetCatalogProductsQueryHandler(IProductRepository products)
    : IRequestHandler<GetCatalogProductsQuery, CatalogProductsListResponse>
{
    public async Task<CatalogProductsListResponse> Handle(
        GetCatalogProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await products
            .GetActivePagedAsync(request.Page, request.PageSize, request.Search, cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(ToDto).ToList();
        return new CatalogProductsListResponse(dtos, request.Page, request.PageSize, total, request.Search);
    }

    private static CatalogProductDto ToDto(Product p) =>
        new(p.Id, p.Sku, p.Name, p.ShortDescription);
}
