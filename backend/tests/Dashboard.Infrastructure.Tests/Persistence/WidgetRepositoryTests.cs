using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using Dashboard.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dashboard.Infrastructure.Tests.Persistence;

public class WidgetRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly DbContextOptions<DashboardDbContext> _options;

    public WidgetRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"dashboard-tests-{Guid.NewGuid()}.db");
        _options = new DbContextOptionsBuilder<DashboardDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        using var context = new DashboardDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private DashboardDbContext CreateContext() => new(_options);

    [Fact]
    public async Task AddAsync_ThenReopenContext_GetAllReturnsPersistedWidget()
    {
        await using (var context = CreateContext())
        {
            var repository = new WidgetRepository(context);
            var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"hi\"}");
            await repository.AddAsync(widget, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verifyContext = CreateContext();
        var verifyRepository = new WidgetRepository(verifyContext);
        var all = await verifyRepository.GetAllAsync(CancellationToken.None);

        var persisted = Assert.Single(all);
        Assert.Equal(WidgetType.Text, persisted.Type);
        Assert.Equal("{\"text\":\"hi\"}", persisted.DataJson);
        Assert.True(persisted.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingWidget_ReturnsIt()
    {
        int id;
        await using (var context = CreateContext())
        {
            var repository = new WidgetRepository(context);
            var widget = Widget.Create(WidgetType.LineChart, order: 0, dataJson: "{\"points\":[]}");
            await repository.AddAsync(widget, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
            id = widget.Id;
        }

        await using var verifyContext = CreateContext();
        var found = await new WidgetRepository(verifyContext).GetByIdAsync(id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(WidgetType.LineChart, found!.Type);
    }

    [Fact]
    public async Task GetByIdAsync_MissingWidget_ReturnsNull()
    {
        await using var context = CreateContext();
        var found = await new WidgetRepository(context).GetByIdAsync(999, CancellationToken.None);

        Assert.Null(found);
    }

    [Fact]
    public async Task Remove_ThenSave_DeletesFromDatabase()
    {
        int id;
        await using (var context = CreateContext())
        {
            var repository = new WidgetRepository(context);
            var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"x\"}");
            await repository.AddAsync(widget, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
            id = widget.Id;
        }

        await using (var context = CreateContext())
        {
            var repository = new WidgetRepository(context);
            var widget = await repository.GetByIdAsync(id, CancellationToken.None);
            repository.Remove(widget!);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verifyContext = CreateContext();
        var stillThere = await new WidgetRepository(verifyContext).GetByIdAsync(id, CancellationToken.None);

        Assert.Null(stillThere);
    }

    [Fact]
    public async Task UpdateText_ThenReopenContext_PersistsAcrossReload()
    {
        int id;
        await using (var context = CreateContext())
        {
            var repository = new WidgetRepository(context);
            var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"old\"}");
            await repository.AddAsync(widget, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
            id = widget.Id;
        }

        await using (var context = CreateContext())
        {
            var repository = new WidgetRepository(context);
            var widget = await repository.GetByIdAsync(id, CancellationToken.None);
            widget!.UpdateText("{\"text\":\"new\"}");
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verifyContext = CreateContext();
        var reloaded = await new WidgetRepository(verifyContext).GetByIdAsync(id, CancellationToken.None);

        Assert.Equal("{\"text\":\"new\"}", reloaded!.DataJson);
    }
}
