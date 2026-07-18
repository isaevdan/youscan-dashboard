using MediatR;

namespace Dashboard.Application.Widgets.Commands.DeleteWidget;

public sealed record DeleteWidgetCommand(int Id) : IRequest;
