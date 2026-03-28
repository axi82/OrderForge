using FluentValidation;
using MediatR;

namespace OrderForge.Application.Organisations;

public sealed record GetOrganisationByIdQuery(int Id) : IRequest<OrganisationDto?>;

public sealed class GetOrganisationByIdQueryValidator : AbstractValidator<GetOrganisationByIdQuery>
{
    public GetOrganisationByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetOrganisationByIdQueryHandler(IOrganisationRepository organisations)
    : IRequestHandler<GetOrganisationByIdQuery, OrganisationDto?>
{
    public async Task<OrganisationDto?> Handle(GetOrganisationByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await organisations.GetByIdAsync(request.Id, cancellationToken);
        return entity?.ToDto();
    }
}
