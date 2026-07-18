using AutoMapper;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using MediatR;

namespace Dashboard.Application.Widgets.Commands.CreateWidget;

public sealed class CreateWidgetCommandHandler : IRequestHandler<CreateWidgetCommand, WidgetDto>
{
    private readonly IWidgetRepository _repository;
    private readonly IRandomDataGenerator _dataGenerator;
    private readonly IMapper _mapper;

    public CreateWidgetCommandHandler(IWidgetRepository repository, IRandomDataGenerator dataGenerator, IMapper mapper)
    {
        _repository = repository;
        _dataGenerator = dataGenerator;
        _mapper = mapper;
    }

    public async Task<WidgetDto> Handle(CreateWidgetCommand request, CancellationToken cancellationToken)
    {
        var type = Enum.Parse<WidgetType>(request.Type, ignoreCase: true);

        var existingWidgets = await _repository.GetAllAsync(cancellationToken);
        var nextOrder = existingWidgets.Count == 0 ? 0 : existingWidgets.Max(w => w.Order) + 1;

        var dataJson = type == WidgetType.Text
            ? "{\"text\":\"\"}"
            : _dataGenerator.GenerateChartDataJson();

        var widget = Widget.Create(type, nextOrder, dataJson);

        await _repository.AddAsync(widget, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WidgetDto>(widget);
    }
}
