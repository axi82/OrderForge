using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Organisations;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public sealed class OrganisationRepository(OrderForgeDbContext dbContext)
    : EfRepository<Organisation>(dbContext), IOrganisationRepository
{
    public override async Task<Organisation?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        var intId = (int)id;
        return await DbContext
            .Set<Organisation>()
            .Include(o => o.OrganisationStatus)
            .FirstOrDefaultAsync(o => o.Id == intId, cancellationToken);
    }

    public override async Task<IReadOnlyList<Organisation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Organisation>()
            .AsNoTracking()
            .Include(o => o.OrganisationStatus)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }
}
