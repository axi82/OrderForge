using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderForge.Application.Common;
using OrderForge.Application.Organisations;
using OrderForge.Domain.Organisations;
using OrderForge.Infrastructure.Persistence;
using OrderForge.Infrastructure.Persistence.Repositories;

namespace OrderForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? configuration.GetConnectionString("orderforge");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<OrderForgeDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IRepository<Organisation>>(sp => sp.GetRequiredService<IOrganisationRepository>());
        services.AddScoped<IOrganisationStatusLookup, OrganisationStatusLookup>();

        return services;
    }
}
