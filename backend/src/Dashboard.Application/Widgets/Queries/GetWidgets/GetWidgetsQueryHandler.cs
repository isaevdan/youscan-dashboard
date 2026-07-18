using Dashboard.Application.Common.Interfaces;
using MediatR;

namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed class GetWidgetsQueryHandler : IRequestHandler<GetWidgetsQuery, IReadOnlyList<WidgetDto>>
{
    private readonly IWidgetRepository _repository;

    public GetWidgetsQueryHandler(IWidgetRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<WidgetDto>> Handle(GetWidgetsQuery request, CancellationToken cancellationToken)
    {
        var widgets = await _repository.GetAllAsync(cancellationToken);

        return widgets
            .OrderBy(w => w.Order)
            .Select(WidgetDto.FromEntity)
            .ToList();
    }
}
