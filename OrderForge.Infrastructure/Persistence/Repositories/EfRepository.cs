using Microsoft.EntityFrameworkCore;
using OrderForge.Application.Common;

namespace OrderForge.Infrastructure.Persistence.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly OrderForgeDbContext DbContext;

    public EfRepository(OrderForgeDbContext dbContext) => DbContext = dbContext;

    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await DbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity) => DbContext.Set<TEntity>().Update(entity);

    public virtual void Remove(TEntity entity) => DbContext.Set<TEntity>().Remove(entity);
}
