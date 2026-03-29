using Microsoft.EntityFrameworkCore;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence;

public class OrderForgeDbContext : DbContext
{
    public OrderForgeDbContext(DbContextOptions<OrderForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<OrganisationStatus> OrganisationStatuses => Set<OrganisationStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderForgeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
