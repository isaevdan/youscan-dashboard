using Dashboard.Domain.Entities;

namespace Dashboard.Application.Common.Interfaces;

public interface IWidgetRepository
{
    Task<IReadOnlyList<Widget>> GetAllAsync(CancellationToken cancellationToken);

    Task<Widget?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Widget widget, CancellationToken cancellationToken);

    void Remove(Widget widget);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
