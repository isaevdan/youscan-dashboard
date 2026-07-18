using AutoMapper;
using Dashboard.Application.Common.Interfaces;
using MediatR;

namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed class GetWidgetsQueryHandler : IRequestHandler<GetWidgetsQuery, IReadOnlyList<WidgetDto>>
{
    private readonly IWidgetRepository _repository;
    private readonly IMapper _mapper;

    public GetWidgetsQueryHandler(IWidgetRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WidgetDto>> Handle(GetWidgetsQuery request, CancellationToken cancellationToken)
    {
        var widgets = await _repository.GetAllAsync(cancellationToken);

        return widgets
            .OrderBy(w => w.Order)
            .Select(_mapper.Map<WidgetDto>)
            .ToList();
    }
}
