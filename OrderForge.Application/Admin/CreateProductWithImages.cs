using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Products;
using OrderForge.Application.Storage;
using OrderForge.Domain.Products;

namespace OrderForge.Application.Admin;

public sealed record CreateProductWithImagesCommand(
    CreateProductCommand Product,
    IReadOnlyList<CreateProductImageUpload> Images,
    int? MainImageIndex) : IRequest<ProductDto>;

public sealed class CreateProductWithImagesCommandValidator : AbstractValidator<CreateProductWithImagesCommand>
{
    public const int MaxImageCount = 20;

    public const long MaxImageBytes = 15 * 1024 * 1024;

    public static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    public CreateProductWithImagesCommandValidator()
    {
        RuleFor(x => x.Product).SetValidator(new CreateProductCommandValidator());

        RuleFor(x => x.Images).NotNull();
        RuleFor(x => x.Images.Count).InclusiveBetween(0, MaxImageCount);

        RuleFor(x => x).Custom(ValidateMainIndex);

        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(i => i.FileName).NotEmpty().MaximumLength(500);
            image
                .RuleFor(i => i.ContentType)
                .Must(ct => ct is null || AllowedContentTypes.Contains(ct))
                .WithMessage("Only JPEG, PNG, and WebP images are allowed.");

            image
                .RuleFor(i => i.Content)
                .Must(s => s is { CanSeek: true, Length: <= MaxImageBytes })
                .WithMessage($"Each image must be seekable and at most {MaxImageBytes} bytes.");
        });
    }

    private static void ValidateMainIndex(CreateProductWithImagesCommand cmd, ValidationContext<CreateProductWithImagesCommand> context)
    {
        if (cmd.Images.Count == 0)
        {
            if (cmd.MainImageIndex is not null)
            {
                context.AddFailure(nameof(cmd.MainImageIndex), "Must be null when no images are uploaded.");
            }

            return;
        }

        if (cmd.MainImageIndex is null
            || cmd.MainImageIndex < 0
            || cmd.MainImageIndex >= cmd.Images.Count)
        {
            context.AddFailure(nameof(cmd.MainImageIndex), "Must select a valid main image index.");
        }
    }
}

public sealed class CreateProductWithImagesCommandHandler(
    IProductRepository products,
    IRepository<ProductImage> productImages,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IBunnyObjectStorage bunnyObjectStorage)
    : IRequestHandler<CreateProductWithImagesCommand, ProductDto>
{
    public Task<ProductDto> Handle(CreateProductWithImagesCommand request, CancellationToken cancellationToken) =>
        HandleCore(request, cancellationToken);

    private async Task<ProductDto> HandleCore(CreateProductWithImagesCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("Authenticated user id is required to create a product.");

        var p = request.Product;
        var sku = p.Sku.Trim();
        if (await products.ExistsWithSkuAsync(sku, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A product with SKU \"{sku}\" already exists.");
        }

        var productCode = p.ProductCode.Trim();
        if (await products.ExistsWithProductCodeAsync(productCode, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A product with product code \"{productCode}\" already exists.");
        }

        var now = DateTime.UtcNow;
        var entity = new Product
        {
            Sku = sku,
            ProductCode = productCode,
            Name = p.Name.Trim(),
            ShortDescription = (p.ShortDescription ?? string.Empty).Trim(),
            Description = string.IsNullOrWhiteSpace(p.Description) ? null : p.Description.Trim(),
            Brand = string.IsNullOrWhiteSpace(p.Brand) ? null : p.Brand.Trim(),
            CommodityCodeDescription = string.IsNullOrWhiteSpace(p.CommodityCodeDescription)
                ? null
                : p.CommodityCodeDescription.Trim(),
            SupplierAccountCode = string.IsNullOrWhiteSpace(p.SupplierAccountCode)
                ? null
                : p.SupplierAccountCode.Trim(),
            PartNumber = string.IsNullOrWhiteSpace(p.PartNumber) ? null : p.PartNumber.Trim(),
            QuantityInStock = p.QuantityInStock,
            QuantityAllocated = p.QuantityAllocated,
            QuantityOnOrder = p.QuantityOnOrder,
            FreeStock = p.FreeStock,
            Barcode = string.IsNullOrWhiteSpace(p.Barcode) ? null : p.Barcode.Trim(),
            CostPrice = p.CostPrice,
            BasePrice = p.BasePrice,
            IsActive = p.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId
        };

        var uploadedPaths = new List<string>();
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await products.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var id = entity.Id;

            for (var i = 0; i < request.Images.Count; i++)
            {
                var img = request.Images[i];
                if (img.Content.CanSeek)
                {
                    img.Content.Position = 0;
                }

                var ext = ExtensionForContentType(img.ContentType);
                var relativePath = $"products/{id}/{Guid.NewGuid():N}{ext}";
                await bunnyObjectStorage
                    .UploadAsync(img.Content, relativePath, img.ContentType, cancellationToken)
                    .ConfigureAwait(false);
                uploadedPaths.Add(relativePath);

                await productImages
                    .AddAsync(
                        new ProductImage
                        {
                            ProductId = id,
                            StoragePath = relativePath,
                            SortOrder = i,
                            IsMain = request.MainImageIndex == i,
                            CreatedAt = DateTime.UtcNow
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            foreach (var path in uploadedPaths)
            {
                try
                {
                    await bunnyObjectStorage.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // best-effort cleanup
                }
            }

            throw;
        }

        var reloaded = await products.GetByIdWithImagesAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        return reloaded!.ToProductDto(bunnyObjectStorage);
    }

    private static string ExtensionForContentType(string? contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
