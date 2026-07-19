using AutoMapper;
using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using MediatR;

namespace Dashboard.Application.Widgets.Commands.CreateWidget;

public sealed class CreateWidgetCommandHandler : IRequestHandler<CreateWidgetCommand, WidgetDto>
{
    // Order assignment is read-max-then-insert; the unique index on Order turns a
    // concurrent-create race into a UniqueConstraintViolationException, which we
    // resolve by re-reading the max order and retrying. With N concurrent writers a
    // request can lose at most N-1 races (every loss means another create committed).
    public const int MaxAttempts = 5;

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
        var type = WidgetTypeParser.Parse(request.Type);

        var dataJson = type == WidgetType.Text
            ? "{\"text\":\"\"}"
            : _dataGenerator.GenerateChartDataJson();

        for (var attempt = 1; ; attempt++)
        {
            var maxOrder = await _repository.GetMaxOrderAsync(cancellationToken);
            var nextOrder = maxOrder.HasValue ? maxOrder.Value + 1 : 0;

            var widget = Widget.Create(type, nextOrder, dataJson);
            await _repository.AddAsync(widget, cancellationToken);

            try
            {
                await _repository.SaveChangesAsync(cancellationToken);
                return _mapper.Map<WidgetDto>(widget);
            }
            catch (UniqueConstraintViolationException) when (attempt < MaxAttempts)
            {
                // Another create won the race for this Order; the repository has
                // discarded the failed insert — recompute the order and try again.
            }
        }
    }
}
