using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderForge.Infrastructure.Persistence;

/// <summary>Used by <c>dotnet ef</c> when the API host is not running.</summary>
public sealed class OrderForgeDbContextFactory : IDesignTimeDbContextFactory<OrderForgeDbContext>
{
    public OrderForgeDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=orderforge;Username=orderforge;Password=orderforge";

        var optionsBuilder = new DbContextOptionsBuilder<OrderForgeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new OrderForgeDbContext(optionsBuilder.Options);
    }
}
