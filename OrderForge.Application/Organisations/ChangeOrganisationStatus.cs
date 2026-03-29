using FluentValidation;
using MediatR;
using OrderForge.Application.Common;

namespace OrderForge.Application.Organisations;

public sealed record ChangeOrganisationStatusCommand(int Id, string Status) : IRequest<OrganisationDto>;

public sealed class ChangeOrganisationStatusCommandValidator : AbstractValidator<ChangeOrganisationStatusCommand>
{
    public ChangeOrganisationStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(code => KnownOrganisationStatusCodes.All.Contains(code))
            .WithMessage($"Status must be one of: {string.Join(", ", KnownOrganisationStatusCodes.All)}.");
    }
}

public sealed class ChangeOrganisationStatusCommandHandler(
    IOrganisationRepository organisations,
    IOrganisationStatusLookup organisationStatuses,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeOrganisationStatusCommand, OrganisationDto>
{
    public async Task<OrganisationDto> Handle(
        ChangeOrganisationStatusCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await organisations.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Organisation {request.Id} was not found.");
        }

        var statusId = await organisationStatuses.GetIdForCodeAsync(request.Status, cancellationToken);
        if (statusId is null)
        {
            throw new InvalidOperationException($"Unknown organisation status code: {request.Status}");
        }

        entity.OrganisationStatusId = statusId.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        organisations.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var reloaded = await organisations.GetByIdAsync(request.Id, cancellationToken);
        return reloaded!.ToDto();
    }
}
