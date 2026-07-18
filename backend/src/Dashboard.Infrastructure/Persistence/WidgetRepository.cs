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

    public async Task<IReadOnlyList<Widget>> GetAllAsync(CancellationToken cancellationToken) =>
        await _context.Widgets.ToListAsync(cancellationToken);

    public Task<Widget?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _context.Widgets.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task AddAsync(Widget widget, CancellationToken cancellationToken) =>
        await _context.Widgets.AddAsync(widget, cancellationToken);

    public void Remove(Widget widget) => _context.Widgets.Remove(widget);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
