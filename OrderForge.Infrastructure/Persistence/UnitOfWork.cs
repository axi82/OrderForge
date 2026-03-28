using OrderForge.Application.Common;

namespace OrderForge.Infrastructure.Persistence;

public sealed class UnitOfWork(OrderForgeDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
