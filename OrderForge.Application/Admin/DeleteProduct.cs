using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Products;

namespace OrderForge.Application.Admin;

public sealed record DeleteProductCommand(int Id) : IRequest;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await products.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Product {request.Id} was not found.");
        }

        products.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
