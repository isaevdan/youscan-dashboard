using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Infrastructure.Persistence;

public class WidgetRepository : IWidgetRepository
{
    private readonly DashboardDbContext _context;

    public WidgetRepository(DashboardDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Widget>> GetPageAsync(int? after, int limit, CancellationToken cancellationToken)
    {
        // Order is unique, but ThenBy(Id) keeps the page order fully deterministic
        // even if that invariant is ever violated (defense in depth).
        var query = _context.Widgets.OrderBy(w => w.Order).ThenBy(w => w.Id).AsQueryable();

        if (after.HasValue)
        {
            query = query.Where(w => w.Order > after.Value);
        }

        return await query.Take(limit).ToListAsync(cancellationToken);
    }

    public async Task<int?> GetMaxOrderAsync(CancellationToken cancellationToken) =>
        await _context.Widgets.MaxAsync(w => (int?)w.Order, cancellationToken);

    public Task<Widget?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Widgets.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task AddAsync(Widget widget, CancellationToken cancellationToken) =>
        await _context.Widgets.AddAsync(widget, cancellationToken);

    public void Remove(Widget widget) => _context.Widgets.Remove(widget);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Detach the failed entries so a caller that retries (e.g. create
            // recomputing the next Order) starts from a clean change tracker.
            foreach (var entry in ex.Entries)
            {
                entry.State = EntityState.Detached;
            }

            throw new UniqueConstraintViolationException(
                "A unique constraint was violated while saving changes.", ex);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        // SQLite error code 19 = SQLITE_CONSTRAINT; extended code 2067 = SQLITE_CONSTRAINT_UNIQUE.
        ex.InnerException is SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 };
}
