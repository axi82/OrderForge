using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrderForge.Client;

namespace OrderForge.Client.Extensions;

public static class AuthorizationPoliciesExtensions
{
    public static IServiceCollection AddOrderForgeAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.SupplierAdmin,
                p => p.RequireRole("SupplierAdmin"));
            options.AddPolicy(
                AuthorizationPolicies.SupplierStaff,
                p => p.RequireRole("SupplierAdmin", "SupplierViewer"));
            options.AddPolicy(
                AuthorizationPolicies.InviteUsers,
                p => p.RequireRole("SupplierAdmin", "CompanyAdmin"));
            options.AddPolicy(
                AuthorizationPolicies.Customer,
                p => p.RequireRole("Customer"));
            options.AddPolicy(
                AuthorizationPolicies.TradeAccount,
                p => p.RequireRole("Customer", "CompanyAdmin"));
        });

        return services;
    }
}
