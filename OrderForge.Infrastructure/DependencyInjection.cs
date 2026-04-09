using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderForge.Application.Common;
using OrderForge.Application.Common.Services;
using OrderForge.Application.Organisations;
using OrderForge.Application.Products;
using OrderForge.Domain.Organisations;
using OrderForge.Domain.Products;
using OrderForge.Infrastructure.Keycloak;
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
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRepository<Product>>(sp => sp.GetRequiredService<IProductRepository>());
        services.AddScoped<IOrganisationStatusLookup, OrganisationStatusLookup>();

        services.Configure<KeycloakAdminOptions>(configuration.GetSection(KeycloakAdminOptions.SectionName));
        services.AddMemoryCache();
        services.AddHttpClient(
            "KeycloakAdmin",
            (sp, client) =>
            {
                var o = sp.GetRequiredService<IOptions<KeycloakAdminOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(o.BaseUrl))
                {
                    client.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
                }
            });
        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

        return services;
    }
}
