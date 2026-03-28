using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Organisations;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public sealed class OrganisationRepository(OrderForgeDbContext dbContext)
    : EfRepository<Organisation>(dbContext), IOrganisationRepository
{
    public override async Task<IReadOnlyList<Organisation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Organisation>()
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }
}
