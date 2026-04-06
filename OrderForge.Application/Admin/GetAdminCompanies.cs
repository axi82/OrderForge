using MediatR;
using OrderForge.Application.Common;
using OrderForge.Application.Organisations;

namespace OrderForge.Application.Admin;

/// <summary>
/// Customer companies linked to Keycloak organisations (for admin UI and invite flows).
/// </summary>
public sealed record GetAdminCompaniesQuery : IRequest<IReadOnlyList<OrganisationDto>>;

public sealed class GetAdminCompaniesQueryHandler(IOrganisationRepository organisations, ICurrentUser currentUser)
    : IRequestHandler<GetAdminCompaniesQuery, IReadOnlyList<OrganisationDto>>
{
    public async Task<IReadOnlyList<OrganisationDto>> Handle(
        GetAdminCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsSupplierAdmin || currentUser.IsSupplierViewer)
        {
            var list = await organisations.GetAllWithKeycloakOrgAsync(cancellationToken);
            return list.Select(o => o.ToDto()).ToList();
        }

        if (currentUser.IsCompanyAdmin && !string.IsNullOrEmpty(currentUser.KeycloakOrganizationId))
        {
            var org = await organisations.GetByKeycloakOrganizationIdAsync(
                currentUser.KeycloakOrganizationId,
                cancellationToken);
            return org is null ? [] : [org.ToDto()];
        }

        return [];
    }
}
