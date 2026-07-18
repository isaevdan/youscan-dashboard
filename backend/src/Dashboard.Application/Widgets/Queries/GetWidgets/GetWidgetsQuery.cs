using MediatR;

namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed record GetWidgetsQuery(int? After, int Limit) : IRequest<WidgetsPageDto>;
