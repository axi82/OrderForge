using MediatR;

namespace OrderForge.Application.Organisations;

public sealed record GetOrganisationsQuery : IRequest<IReadOnlyList<OrganisationDto>>;

public sealed class GetOrganisationsQueryHandler(IOrganisationRepository organisations)
    : IRequestHandler<GetOrganisationsQuery, IReadOnlyList<OrganisationDto>>
{
    public async Task<IReadOnlyList<OrganisationDto>> Handle(
        GetOrganisationsQuery request,
        CancellationToken cancellationToken)
    {
        var list = await organisations.GetAllAsync(cancellationToken);
        return list.Select(o => o.ToDto()).ToList();
    }
}
