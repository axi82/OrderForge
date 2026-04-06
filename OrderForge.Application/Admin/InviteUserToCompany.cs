using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;
using OrderForge.Application.Organisations;

namespace OrderForge.Application.Admin;

public sealed record InviteUserToCompanyCommand(
    int OrganisationId,
    string Email,
    string? FirstName,
    string? LastName,
    string RealmRoleName,
    string? TemporaryPassword) : IRequest<InviteUserToCompanyResult>;

public sealed record InviteUserToCompanyResult(string UserId, string Email, string Username);

public sealed class InviteUserToCompanyCommandValidator : AbstractValidator<InviteUserToCompanyCommand>
{
    public InviteUserToCompanyCommandValidator()
    {
        RuleFor(x => x.OrganisationId).GreaterThan(0);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.RealmRoleName)
            .NotEmpty()
            .Must(n => KnownRealmRoles.CompanyInviteRoles.Contains(n, StringComparer.Ordinal))
            .WithMessage($"Role must be one of: {string.Join(", ", KnownRealmRoles.CompanyInviteRoles)}.");
        RuleFor(x => x.TemporaryPassword).MinimumLength(8).When(x => x.TemporaryPassword is not null);
    }
}

public sealed class InviteUserToCompanyCommandHandler(
    IOrganisationRepository organisations,
    IKeycloakAdminService keycloakAdmin,
    ICurrentUser currentUser,
    ILogger<InviteUserToCompanyCommandHandler> logger)
    : IRequestHandler<InviteUserToCompanyCommand, InviteUserToCompanyResult>
{
    public async Task<InviteUserToCompanyResult> Handle(
        InviteUserToCompanyCommand request,
        CancellationToken cancellationToken)
    {
        var org = await organisations.GetByIdAsync(request.OrganisationId, cancellationToken);
        if (org is null)
        {
            throw new KeyNotFoundException($"Organisation {request.OrganisationId} was not found.");
        }

        if (string.IsNullOrEmpty(org.KeycloakOrganizationId))
        {
            throw new InvalidOperationException(
                "This company is not linked to Keycloak yet. Create or repair the company record first.");
        }

        if (!CanInviteToOrganisation(org.KeycloakOrganizationId))
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to invite users for this company.");
        }

        var invite = new InviteUserToCompanyRequest(
            org.KeycloakOrganizationId,
            request.Email,
            request.FirstName,
            request.LastName,
            request.RealmRoleName,
            request.TemporaryPassword);

        var created = await keycloakAdmin.InviteUserToCompanyAsync(invite, cancellationToken);

        logger.LogInformation(
            "Invited user {Email} to organisation {OrganisationId} (Keycloak org {KeycloakOrgId}) as {Role} by {Actor}",
            request.Email,
            request.OrganisationId,
            org.KeycloakOrganizationId,
            request.RealmRoleName,
            currentUser.UserId ?? "(unknown)");

        return new InviteUserToCompanyResult(created.UserId, request.Email, created.Username);
    }

    private bool CanInviteToOrganisation(string keycloakOrganizationId)
    {
        if (currentUser.IsSupplierAdmin)
        {
            return true;
        }

        if (currentUser.IsCompanyAdmin
            && !string.IsNullOrEmpty(currentUser.KeycloakOrganizationId)
            && string.Equals(
                currentUser.KeycloakOrganizationId,
                keycloakOrganizationId,
                StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
