using Dashboard.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Dashboard.Infrastructure.Tests.Persistence;

/// <summary>
/// Verifies the AddUniqueOrderIndex migration against a database that already
/// contains colliding Order values (as produced by the pre-index create race):
/// colliding rows must be deterministically reassigned before the unique index
/// is created, so the migration cannot fail on an existing database.
/// </summary>
public class AddUniqueOrderIndexMigrationTests : IDisposable
{
    private const string InitialCreateMigration = "20260717105522_InitialCreate";

    private readonly string _dbPath;
    private readonly DbContextOptions<DashboardDbContext> _options;

    public AddUniqueOrderIndexMigrationTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"dashboard-migration-tests-{Guid.NewGuid()}.db");
        _options = new DbContextOptionsBuilder<DashboardDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task Migrate_DatabaseWithDuplicateOrders_DeduplicatesThenCreatesUniqueIndex()
    {
        await using (var context = new DashboardDbContext(_options))
        {
            // Bring the schema to the state before the unique index existed,
            // then simulate the duplicates the create race used to produce.
            await context.GetService<IMigrator>().MigrateAsync(InitialCreateMigration);
            // Braces are doubled because ExecuteSqlRaw treats {n} as a parameter placeholder.
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO "Widgets" ("Type", "Order", "DataJson") VALUES
                ('Text', 0, '{{"text":"id-1"}}'),
                ('Text', 0, '{{"text":"id-2"}}'),
                ('Text', 1, '{{"text":"id-3"}}'),
                ('Text', 0, '{{"text":"id-4"}}');
                """);
        }

        await using (var context = new DashboardDbContext(_options))
        {
            await context.Database.MigrateAsync();
        }

        await using var verifyContext = new DashboardDbContext(_options);
        var widgets = await verifyContext.Widgets.OrderBy(w => w.Id).ToListAsync();

        // Lowest Id per colliding Order keeps its value; the rest move to the next
        // free Order values (max + 1, max + 2, ...) in Id order.
        Assert.Equal([0, 2, 1, 3], widgets.Select(w => w.Order));

        // And the unique index is in place: another Order collision now fails.
        await verifyContext.Database.ExecuteSqlRawAsync(
            """INSERT INTO "Widgets" ("Type", "Order", "DataJson") VALUES ('Text', 5, '{{"text":""}}')""");
        await Assert.ThrowsAsync<SqliteException>(() => verifyContext.Database.ExecuteSqlRawAsync(
            """INSERT INTO "Widgets" ("Type", "Order", "DataJson") VALUES ('Text', 5, '{{"text":""}}')"""));
    }
}
