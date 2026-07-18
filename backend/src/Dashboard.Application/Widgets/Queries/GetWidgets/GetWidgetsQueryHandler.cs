using AutoMapper;
using Dashboard.Application.Common.Interfaces;
using MediatR;

namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed class GetWidgetsQueryHandler : IRequestHandler<GetWidgetsQuery, WidgetsPageDto>
{
    private readonly IWidgetRepository _repository;
    private readonly IMapper _mapper;

    public GetWidgetsQueryHandler(IWidgetRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<WidgetsPageDto> Handle(GetWidgetsQuery request, CancellationToken cancellationToken)
    {
        var widgets = await _repository.GetPageAsync(request.After, request.Limit + 1, cancellationToken);

        var hasMore = widgets.Count > request.Limit;
        var page = hasMore ? widgets.Take(request.Limit).ToList() : widgets;

        var items = page.Select(_mapper.Map<WidgetDto>).ToList();
        var nextCursor = hasMore ? (int?)page[^1].Order : null;

        return new WidgetsPageDto(items, hasMore, nextCursor);
    }
}
