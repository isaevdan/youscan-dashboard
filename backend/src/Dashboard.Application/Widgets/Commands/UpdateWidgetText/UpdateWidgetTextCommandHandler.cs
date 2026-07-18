using System.Text.Json;
using AutoMapper;
using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Domain.Entities;
using MediatR;

namespace Dashboard.Application.Widgets.Commands.UpdateWidgetText;

public sealed class UpdateWidgetTextCommandHandler : IRequestHandler<UpdateWidgetTextCommand, WidgetDto>
{
    private readonly IWidgetRepository _repository;
    private readonly IMapper _mapper;

    public UpdateWidgetTextCommandHandler(IWidgetRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<WidgetDto> Handle(UpdateWidgetTextCommand request, CancellationToken cancellationToken)
    {
        var widget = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Widget), request.Id);

        var dataJson = JsonSerializer.Serialize(new { text = request.Text });
        widget.UpdateText(dataJson);

        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WidgetDto>(widget);
    }
}
