using Dashboard.Application.Common.Interfaces;
using Dashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Connection string is resolved lazily from DI when the DbContext is created (per scope),
        // not captured eagerly here - so config overrides applied after registration (e.g. by
        // WebApplicationFactory in tests) still take effect.
        services.AddDbContext<DashboardDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Default")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings is not configured");
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IWidgetRepository, WidgetRepository>();
        services.AddSingleton<IRandomDataGenerator, RandomDataGenerator>();

        return services;
    }
}
