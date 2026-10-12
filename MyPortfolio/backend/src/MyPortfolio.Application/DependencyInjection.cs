using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyPortfolio.Application.Common.Behaviors;

namespace MyPortfolio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register FluentValidation validators via assembly scanning
        services.AddValidatorsFromAssembly(assembly);

        // Register MediatR handlers and pipeline behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
