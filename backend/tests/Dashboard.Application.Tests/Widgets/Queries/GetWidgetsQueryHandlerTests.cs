using Dashboard.Application.Common.Interfaces;
using Dashboard.Application.Widgets.Queries.GetWidgets;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Queries;

public class GetWidgetsQueryHandlerTests
{
    private readonly IWidgetRepository _repository = Substitute.For<IWidgetRepository>();

    private GetWidgetsQueryHandler CreateHandler() => new(_repository);

    [Fact]
    public async Task Handle_ReturnsWidgetsOrderedByPosition()
    {
        var widgets = new List<Widget>
        {
            Widget.Create(WidgetType.BarChart, order: 1, dataJson: "{\"points\":[]}"),
            Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"\"}")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(widgets);

        var result = await CreateHandler().Handle(new GetWidgetsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(WidgetType.Text, result[0].Type);
        Assert.Equal(WidgetType.BarChart, result[1].Type);
    }

    [Fact]
    public async Task Handle_NoWidgets_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Widget>());

        var result = await CreateHandler().Handle(new GetWidgetsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
