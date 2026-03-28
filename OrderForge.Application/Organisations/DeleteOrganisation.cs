using FluentValidation;
using MediatR;
using OrderForge.Application.Common;

namespace OrderForge.Application.Organisations;

public sealed record DeleteOrganisationCommand(int Id) : IRequest;

public sealed class DeleteOrganisationCommandValidator : AbstractValidator<DeleteOrganisationCommand>
{
    public DeleteOrganisationCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteOrganisationCommandHandler(
    IOrganisationRepository organisations,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOrganisationCommand>
{
    public async Task Handle(DeleteOrganisationCommand request, CancellationToken cancellationToken)
    {
        var entity = await organisations.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Organisation {request.Id} was not found.");
        }

        organisations.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
