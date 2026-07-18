namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed record WidgetsPageDto(IReadOnlyList<WidgetDto> Items, bool HasMore, int? NextCursor);
