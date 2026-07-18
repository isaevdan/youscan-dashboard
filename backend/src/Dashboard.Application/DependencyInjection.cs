using Dashboard.Application.Common.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        return services;
    }
}
