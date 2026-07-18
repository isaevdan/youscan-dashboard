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

    private GetWidgetsQueryHandler CreateHandler() => new(_repository, TestMapper.Instance);

    [Fact]
    public async Task Handle_FewerWidgetsThanLimit_ReturnsAllWithHasMoreFalse()
    {
        var widgets = new List<Widget>
        {
            Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"\"}"),
            Widget.Create(WidgetType.BarChart, order: 1, dataJson: "{\"points\":[]}")
        };
        _repository.GetPageAsync(null, 11, Arg.Any<CancellationToken>()).Returns(widgets);

        var result = await CreateHandler().Handle(new GetWidgetsQuery(After: null, Limit: 10), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(WidgetType.Text, result.Items[0].Type);
        Assert.Equal(WidgetType.BarChart, result.Items[1].Type);
        Assert.False(result.HasMore);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task Handle_NoWidgets_ReturnsEmptyItemsAndHasMoreFalse()
    {
        _repository.GetPageAsync(null, 11, Arg.Any<CancellationToken>()).Returns(new List<Widget>());

        var result = await CreateHandler().Handle(new GetWidgetsQuery(After: null, Limit: 10), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task Handle_MoreWidgetsThanLimit_ReturnsOnlyLimitAndSetsHasMoreAndNextCursor()
    {
        var widgets = Enumerable.Range(0, 4)
            .Select(i => Widget.Create(WidgetType.Text, order: i, dataJson: "{\"text\":\"\"}"))
            .ToList();
        _repository.GetPageAsync(null, 4, Arg.Any<CancellationToken>()).Returns(widgets);

        var result = await CreateHandler().Handle(new GetWidgetsQuery(After: null, Limit: 3), CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.True(result.HasMore);
        Assert.Equal(2, result.NextCursor);
    }

    [Fact]
    public async Task Handle_PassesAfterCursorAndLimitPlusOneToRepository()
    {
        _repository.GetPageAsync(5, 11, Arg.Any<CancellationToken>()).Returns(new List<Widget>());

        await CreateHandler().Handle(new GetWidgetsQuery(After: 5, Limit: 10), CancellationToken.None);

        await _repository.Received(1).GetPageAsync(5, 11, Arg.Any<CancellationToken>());
    }
}
