using Dashboard.Application.Common.Interfaces;
using Dashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=dashboard.db";

        services.AddDbContext<DashboardDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IWidgetRepository, WidgetRepository>();
        services.AddSingleton<IRandomDataGenerator, RandomDataGenerator>();

        return services;
    }
}
