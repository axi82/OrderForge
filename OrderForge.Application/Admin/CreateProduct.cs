using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Products;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Admin;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? ShortDescription,
    string? Description,
    string? Brand,
    decimal CostPrice,
    decimal BasePrice,
    bool IsActive) : IRequest<ProductDto>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ShortDescription).MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(20000);
        RuleFor(x => x.Brand).MaximumLength(200);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProductCommandHandler(
    IProductRepository products,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("Authenticated user id is required to create a product.");

        var sku = request.Sku.Trim();
        if (await products.ExistsWithSkuAsync(sku, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A product with SKU \"{sku}\" already exists.");
        }

        var now = DateTime.UtcNow;
        var entity = new Product
        {
            Sku = sku,
            Name = request.Name.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(request.ShortDescription)
                ? null
                : request.ShortDescription.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
            CostPrice = request.CostPrice,
            BasePrice = request.BasePrice,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId
        };

        await products.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ProductDto(
            entity.Id,
            entity.Sku,
            entity.Name,
            entity.ShortDescription,
            entity.Description,
            entity.Brand,
            entity.CostPrice,
            entity.BasePrice,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedBy);
    }
}
