using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Application.Widgets.Commands.UpdateWidgetText;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using Dashboard.Domain.Exceptions;
using NSubstitute;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Commands;

public class UpdateWidgetTextCommandHandlerTests
{
    private readonly IWidgetRepository _repository = Substitute.For<IWidgetRepository>();

    private UpdateWidgetTextCommandHandler CreateHandler() => new(_repository);

    [Fact]
    public async Task Handle_ExistingTextWidget_UpdatesAndSaves()
    {
        var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"old\"}");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(widget);

        var result = await CreateHandler().Handle(new UpdateWidgetTextCommand(1, "new"), CancellationToken.None);

        Assert.Contains("new", result.Data.GetProperty("text").GetString());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingWidget_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Widget?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new UpdateWidgetTextCommand(99, "new"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ChartWidget_ThrowsInvalidWidgetOperationException()
    {
        var widget = Widget.Create(WidgetType.LineChart, order: 0, dataJson: "{\"points\":[]}");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(widget);

        await Assert.ThrowsAsync<InvalidWidgetOperationException>(
            () => CreateHandler().Handle(new UpdateWidgetTextCommand(1, "new"), CancellationToken.None));
    }
}
