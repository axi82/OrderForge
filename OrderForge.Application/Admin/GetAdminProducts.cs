using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Products;
using OrderForge.Application.Storage;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Admin;

public sealed record AdminProductsListResponse(
    IReadOnlyList<ProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search);

public sealed record GetAdminProductsQuery(int Page, int PageSize, string? Search)
    : IRequest<AdminProductsListResponse>;

public sealed class GetAdminProductsQueryValidator : AbstractValidator<GetAdminProductsQuery>
{
    public GetAdminProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200).When(x => x.Search is not null);
    }
}

public sealed class GetAdminProductsQueryHandler(
    IProductRepository products,
    ICurrentUser currentUser,
    IBunnyObjectStorage bunnyObjectStorage)
    : IRequestHandler<GetAdminProductsQuery, AdminProductsListResponse>
{
    public async Task<AdminProductsListResponse> Handle(
        GetAdminProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsSupplierAdmin && !currentUser.IsSupplierViewer)
        {
            return new AdminProductsListResponse([], request.Page, request.PageSize, 0, request.Search);
        }

        var (items, total) = await products
            .GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(p => p.ToProductDto(bunnyObjectStorage)).ToList();
        return new AdminProductsListResponse(dtos, request.Page, request.PageSize, total, request.Search);
    }
}
