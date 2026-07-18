using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Domain.Entities;
using MediatR;

namespace Dashboard.Application.Widgets.Commands.DeleteWidget;

public sealed class DeleteWidgetCommandHandler : IRequestHandler<DeleteWidgetCommand>
{
    private readonly IWidgetRepository _repository;

    public DeleteWidgetCommandHandler(IWidgetRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteWidgetCommand request, CancellationToken cancellationToken)
    {
        var widget = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Widget), request.Id);

        _repository.Remove(widget);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
