using MediatR;

namespace Dashboard.Application.Widgets.Commands.UpdateWidgetText;

public sealed record UpdateWidgetTextCommand(int Id, string Text) : IRequest<WidgetDto>;
