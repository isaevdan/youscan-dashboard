using Dashboard.Application.Common.Interfaces;
using Dashboard.Domain.Entities;
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
        var query = _context.Widgets.OrderBy(w => w.Order).AsQueryable();

        if (after.HasValue)
        {
            query = query.Where(w => w.Order > after.Value);
        }

        return await query.Take(limit).ToListAsync(cancellationToken);
    }

    public async Task<int?> GetMaxOrderAsync(CancellationToken cancellationToken)
    {
        if (!await _context.Widgets.AnyAsync(cancellationToken))
        {
            return null;
        }

        return await _context.Widgets.MaxAsync(w => w.Order, cancellationToken);
    }

    public Task<Widget?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Widgets.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task AddAsync(Widget widget, CancellationToken cancellationToken) =>
        await _context.Widgets.AddAsync(widget, cancellationToken);

    public void Remove(Widget widget) => _context.Widgets.Remove(widget);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
