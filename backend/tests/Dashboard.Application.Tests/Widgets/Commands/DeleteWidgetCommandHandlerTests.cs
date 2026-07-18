using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Application.Widgets.Commands.DeleteWidget;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Commands;

public class DeleteWidgetCommandHandlerTests
{
    private readonly IWidgetRepository _repository = Substitute.For<IWidgetRepository>();

    private DeleteWidgetCommandHandler CreateHandler() => new(_repository);

    [Fact]
    public async Task Handle_ExistingWidget_RemovesAndSaves()
    {
        var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"x\"}");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(widget);

        await CreateHandler().Handle(new DeleteWidgetCommand(1), CancellationToken.None);

        _repository.Received(1).Remove(widget);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingWidget_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Widget?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new DeleteWidgetCommand(99), CancellationToken.None));
    }
}
