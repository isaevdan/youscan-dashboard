using Dashboard.Domain.Entities;

namespace Dashboard.Application.Common.Interfaces;

public interface IWidgetRepository
{
    /// <summary>Widgets ordered by Order ascending, optionally after a given Order cursor, up to limit.</summary>
    Task<IReadOnlyList<Widget>> GetPageAsync(int? after, int limit, CancellationToken cancellationToken);

    /// <summary>The highest Order currently in use, or null if there are no widgets.</summary>
    Task<int?> GetMaxOrderAsync(CancellationToken cancellationToken);

    Task<Widget?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Widget widget, CancellationToken cancellationToken);

    void Remove(Widget widget);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
