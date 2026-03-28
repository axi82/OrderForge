using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderForge.Application.Behaviors;
using OrderForge.Application.Organisations;

namespace OrderForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(CreateOrganisationCommand).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
