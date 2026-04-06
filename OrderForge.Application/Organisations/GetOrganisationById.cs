using FluentValidation;
using MediatR;
using OrderForge.Application.Common;

namespace OrderForge.Application.Organisations;

public sealed record GetOrganisationByIdQuery(int Id) : IRequest<OrganisationDto?>;

public sealed class GetOrganisationByIdQueryValidator : AbstractValidator<GetOrganisationByIdQuery>
{
    public GetOrganisationByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetOrganisationByIdQueryHandler(IOrganisationRepository organisations, ICurrentUser currentUser)
    : IRequestHandler<GetOrganisationByIdQuery, OrganisationDto?>
{
    public async Task<OrganisationDto?> Handle(GetOrganisationByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await organisations.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (!OrganisationVisibility.CanView(currentUser, entity))
        {
            return null;
        }

        return entity.ToDto();
    }
}
