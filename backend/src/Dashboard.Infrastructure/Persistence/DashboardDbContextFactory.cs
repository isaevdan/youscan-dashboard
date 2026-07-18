using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dashboard.Infrastructure.Persistence;

/// <summary>Design-time only, used by `dotnet ef migrations add` before Dashboard.Api wires up DI.</summary>
public class DashboardDbContextFactory : IDesignTimeDbContextFactory<DashboardDbContext>
{
    public DashboardDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DashboardDbContext>()
            .UseSqlite("Data Source=dashboard.db")
            .Options;

        return new DashboardDbContext(options);
    }
}
