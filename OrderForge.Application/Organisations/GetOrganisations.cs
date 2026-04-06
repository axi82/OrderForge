using MediatR;
using OrderForge.Application.Common;
using OrderForge.Domain.Organisations;

namespace OrderForge.Application.Organisations;

public sealed record GetOrganisationsQuery : IRequest<IReadOnlyList<OrganisationDto>>;

public sealed class GetOrganisationsQueryHandler(IOrganisationRepository organisations, ICurrentUser currentUser)
    : IRequestHandler<GetOrganisationsQuery, IReadOnlyList<OrganisationDto>>
{
    public async Task<IReadOnlyList<OrganisationDto>> Handle(
        GetOrganisationsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Organisation> list;
        if (currentUser.IsSupplierAdmin || currentUser.IsSupplierViewer)
        {
            list = await organisations.GetAllAsync(cancellationToken);
        }
        else if (currentUser.IsCompanyAdmin && !string.IsNullOrEmpty(currentUser.KeycloakOrganizationId))
        {
            var org = await organisations.GetByKeycloakOrganizationIdAsync(
                currentUser.KeycloakOrganizationId,
                cancellationToken);
            list = org is null ? [] : [org];
        }
        else
        {
            list = [];
        }

        return list.Select(o => o.ToDto()).ToList();
    }
}
