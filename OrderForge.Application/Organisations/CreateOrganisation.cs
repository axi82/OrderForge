using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Organisations;

public sealed record CreateOrganisationCommand(
    string Name,
    string? TradingAs,
    string? CompanyNumber,
    string? VatNumber,
    string? AccountNumber,
    string Status = KnownOrganisationStatusCodes.Active) : IRequest<OrganisationDto>;

public sealed class CreateOrganisationCommandValidator : AbstractValidator<CreateOrganisationCommand>
{
    public CreateOrganisationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TradingAs).MaximumLength(200);
        RuleFor(x => x.CompanyNumber).MaximumLength(50);
        RuleFor(x => x.VatNumber).MaximumLength(20);
        RuleFor(x => x.AccountNumber).MaximumLength(20);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(code => KnownOrganisationStatusCodes.All.Contains(code))
            .WithMessage($"Status must be one of: {string.Join(", ", KnownOrganisationStatusCodes.All)}.");
    }
}

public sealed class CreateOrganisationCommandHandler(
    IOrganisationRepository organisations,
    IOrganisationStatusLookup organisationStatuses,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrganisationCommand, OrganisationDto>
{
    public async Task<OrganisationDto> Handle(CreateOrganisationCommand request, CancellationToken cancellationToken)
    {
        var statusId = await organisationStatuses.GetIdForCodeAsync(request.Status, cancellationToken);
        if (statusId is null)
        {
            throw new InvalidOperationException($"Unknown organisation status code: {request.Status}");
        }

        var now = DateTime.UtcNow;
        var entity = new Organisation
        {
            Name = request.Name,
            TradingAs = request.TradingAs,
            CompanyNumber = request.CompanyNumber,
            VatNumber = request.VatNumber,
            AccountNumber = request.AccountNumber,
            OrganisationStatusId = statusId.Value,
            CreatedAt = now,
            UpdatedAt = now
        };

        await organisations.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var reloaded = await organisations.GetByIdAsync(entity.Id, cancellationToken);
        return reloaded!.ToDto();
    }
}
