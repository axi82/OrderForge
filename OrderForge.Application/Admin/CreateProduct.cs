using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Products;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Admin;

public sealed record CreateProductCommand(
    string Sku,
    string ProductCode,
    string Name,
    string? ShortDescription,
    string? Description,
    string? Brand,
    string? CommodityCodeDescription,
    string? SupplierAccountCode,
    string? PartNumber,
    decimal QuantityInStock,
    decimal QuantityAllocated,
    decimal QuantityOnOrder,
    decimal FreeStock,
    string? Barcode,
    decimal CostPrice,
    decimal BasePrice,
    bool IsActive) : IRequest<ProductDto>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ShortDescription).MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(20000);
        RuleFor(x => x.Brand).MaximumLength(200);
        RuleFor(x => x.CommodityCodeDescription).MaximumLength(100);
        RuleFor(x => x.SupplierAccountCode).MaximumLength(50);
        RuleFor(x => x.PartNumber).MaximumLength(100);
        RuleFor(x => x.Barcode).MaximumLength(64);
        RuleFor(x => x.QuantityInStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantityAllocated).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantityOnOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FreeStock).GreaterThanOrEqualTo(0);
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

        var productCode = request.ProductCode.Trim();
        if (await products.ExistsWithProductCodeAsync(productCode, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A product with product code \"{productCode}\" already exists.");
        }

        var now = DateTime.UtcNow;
        var entity = new Product
        {
            Sku = sku,
            ProductCode = productCode,
            Name = request.Name.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(request.ShortDescription)
                ? null
                : request.ShortDescription.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
            CommodityCodeDescription = string.IsNullOrWhiteSpace(request.CommodityCodeDescription)
                ? null
                : request.CommodityCodeDescription.Trim(),
            SupplierAccountCode = string.IsNullOrWhiteSpace(request.SupplierAccountCode)
                ? null
                : request.SupplierAccountCode.Trim(),
            PartNumber = string.IsNullOrWhiteSpace(request.PartNumber) ? null : request.PartNumber.Trim(),
            QuantityInStock = request.QuantityInStock,
            QuantityAllocated = request.QuantityAllocated,
            QuantityOnOrder = request.QuantityOnOrder,
            FreeStock = request.FreeStock,
            Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim(),
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
            entity.ProductCode,
            entity.Name,
            entity.ShortDescription,
            entity.Description,
            entity.Brand,
            entity.CommodityCodeDescription,
            entity.SupplierAccountCode,
            entity.PartNumber,
            entity.QuantityInStock,
            entity.QuantityAllocated,
            entity.QuantityOnOrder,
            entity.FreeStock,
            entity.Barcode,
            entity.CostPrice,
            entity.BasePrice,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedBy);
    }
}
