using MediatR;

namespace Dashboard.Application.Widgets.Commands.CreateWidget;

public sealed record CreateWidgetCommand(string Type) : IRequest<WidgetDto>;
