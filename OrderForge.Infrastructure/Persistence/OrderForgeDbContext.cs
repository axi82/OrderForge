using Microsoft.EntityFrameworkCore;
using OrderForge.Domain.Organisations;
using OrderForge.Domain.Orders;
using OrderForge.Domain.Products;

namespace OrderForge.Infrastructure.Persistence;

public class OrderForgeDbContext : DbContext
{
    public OrderForgeDbContext(DbContextOptions<OrderForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<OrganisationStatus> OrganisationStatuses => Set<OrganisationStatus>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<TradeOrder> TradeOrders => Set<TradeOrder>();

    public DbSet<TradeOrderLine> TradeOrderLines => Set<TradeOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderForgeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
