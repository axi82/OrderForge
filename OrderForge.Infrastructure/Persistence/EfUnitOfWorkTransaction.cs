using Microsoft.EntityFrameworkCore.Storage;
using OrderForge.Application.Common;

namespace OrderForge.Infrastructure.Persistence;

public sealed class EfUnitOfWorkTransaction(IDbContextTransaction inner) : IUnitOfWorkTransaction
{
    private bool _disposed;

    public Task CommitAsync(CancellationToken cancellationToken = default) => inner.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) => inner.RollbackAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await inner.DisposeAsync().ConfigureAwait(false);
    }
}
