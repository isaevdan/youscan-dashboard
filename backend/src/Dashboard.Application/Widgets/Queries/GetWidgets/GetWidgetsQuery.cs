using MediatR;

namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed record GetWidgetsQuery : IRequest<IReadOnlyList<WidgetDto>>;
