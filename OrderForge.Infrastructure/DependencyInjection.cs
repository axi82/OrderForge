using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderForge.Infrastructure.Persistence;

namespace OrderForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<OrderForgeDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
