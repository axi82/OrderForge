using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;
using OrderForge.Application.Organisations;
using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Admin;

public sealed record CreateCustomerCompanyCommand(
    string Name,
    string? TradingAs,
    string? CompanyNumber,
    string? VatNumber,
    string? AccountNumber,
    string Status = KnownOrganisationStatusCodes.Active) : IRequest<OrganisationDto>;

public sealed class CreateCustomerCompanyCommandValidator : AbstractValidator<CreateCustomerCompanyCommand>
{
    public CreateCustomerCompanyCommandValidator()
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

public sealed class CreateCustomerCompanyCommandHandler(
    IOrganisationRepository organisations,
    IOrganisationStatusLookup organisationStatuses,
    IKeycloakAdminService keycloakAdmin,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<CreateCustomerCompanyCommandHandler> logger)
    : IRequestHandler<CreateCustomerCompanyCommand, OrganisationDto>
{
    public async Task<OrganisationDto> Handle(
        CreateCustomerCompanyCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsSupplierAdmin)
        {
            throw new UnauthorizedAccessException("Only a SupplierAdmin can create customer companies.");
        }

        if (await organisations.ExistsWithNameAsync(request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"A company named '{request.Name}' already exists.");
        }

        var displayName = string.IsNullOrWhiteSpace(request.TradingAs) ? request.Name : request.TradingAs;
        var kcOrg = await keycloakAdmin.CreateOrganizationAsync(request.Name, displayName, cancellationToken);

        var statusId = await organisationStatuses.GetIdForCodeAsync(request.Status, cancellationToken);
        if (statusId is null)
        {
            throw new InvalidOperationException($"Unknown organisation status code: {request.Status}");
        }

        var now = DateTime.UtcNow;
        var entity = new Organisation
        {
            Name = request.Name,
            KeycloakOrganizationId = kcOrg.Id,
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

        logger.LogInformation(
            "Created customer company {Name} with Keycloak organisation {KeycloakOrgId} by {Actor}",
            request.Name,
            kcOrg.Id,
            currentUser.UserId ?? "(unknown)");

        var reloaded = await organisations.GetByIdAsync(entity.Id, cancellationToken);
        return reloaded!.ToDto();
    }
}
