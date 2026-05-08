using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Common;

namespace OrderForge.Infrastructure.Persistence;

public sealed class UnitOfWork(OrderForgeDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        return new EfUnitOfWorkTransaction(tx);
    }
}
