using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Organisations;

namespace OrderForge.Infrastructure.Persistence;

public sealed class OrganisationStatusLookup(OrderForgeDbContext db) : IOrganisationStatusLookup
{
    public async Task<int?> GetIdForCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var trimmed = code.Trim();
        return await db
            .OrganisationStatuses.AsNoTracking()
            .Where(s => s.Code == trimmed)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
