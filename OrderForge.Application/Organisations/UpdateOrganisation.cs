using FluentValidation;
using MediatR;
using OrderForge.Application.Common;
using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Organisations;

public sealed record UpdateOrganisationCommand(
    int Id,
    string Name,
    string? TradingAs,
    string? CompanyNumber,
    string? VatNumber,
    string? AccountNumber,
    string Status) : IRequest<OrganisationDto>;

public sealed class UpdateOrganisationCommandValidator : AbstractValidator<UpdateOrganisationCommand>
{
    public UpdateOrganisationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TradingAs).MaximumLength(200);
        RuleFor(x => x.CompanyNumber).MaximumLength(50);
        RuleFor(x => x.VatNumber).MaximumLength(20);
        RuleFor(x => x.AccountNumber).MaximumLength(20);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(20);
    }
}

public sealed class UpdateOrganisationCommandHandler(
    IOrganisationRepository organisations,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateOrganisationCommand, OrganisationDto>
{
    public async Task<OrganisationDto> Handle(UpdateOrganisationCommand request, CancellationToken cancellationToken)
    {
        var entity = await organisations.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Organisation {request.Id} was not found.");
        }

        entity.Name = request.Name;
        entity.TradingAs = request.TradingAs;
        entity.CompanyNumber = request.CompanyNumber;
        entity.VatNumber = request.VatNumber;
        entity.AccountNumber = request.AccountNumber;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        organisations.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }
}
