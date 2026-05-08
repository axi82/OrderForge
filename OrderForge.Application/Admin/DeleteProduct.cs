using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Products;
using OrderForge.Application.Storage;

namespace OrderForge.Application.Admin;

public sealed record DeleteProductCommand(int Id) : IRequest;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteProductCommandHandler(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IBunnyObjectStorage bunnyObjectStorage)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await products
            .GetByIdWithImagesAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Product {request.Id} was not found.");
        }

        foreach (var image in entity.Images)
        {
            try
            {
                await bunnyObjectStorage.DeleteAsync(image.StoragePath, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // best-effort; DB delete still proceeds
            }
        }

        products.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
