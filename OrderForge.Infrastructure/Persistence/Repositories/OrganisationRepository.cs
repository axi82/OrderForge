using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Organisations;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public sealed class OrganisationRepository(OrderForgeDbContext dbContext)
    : EfRepository<Organisation>(dbContext), IOrganisationRepository
{
    public async Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Organisation>()
            .AsNoTracking()
            .AnyAsync(o => o.Name == name, cancellationToken);
    }

    public async Task<Organisation?> GetByKeycloakOrganizationIdAsync(
        string keycloakOrganizationId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Organisation>()
            .AsNoTracking()
            .Include(o => o.OrganisationStatus)
            .FirstOrDefaultAsync(o => o.KeycloakOrganizationId == keycloakOrganizationId, cancellationToken);
    }

    public async Task<IReadOnlyList<Organisation>> GetAllWithKeycloakOrgAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Organisation>()
            .AsNoTracking()
            .Include(o => o.OrganisationStatus)
            .Where(o => o.KeycloakOrganizationId != null)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

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
