using Dashboard.Application.Common.Exceptions;
using Dashboard.Application.Common.Interfaces;
using Dashboard.Application.Widgets.Commands.CreateWidget;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Commands;

public class CreateWidgetCommandHandlerTests
{
    private readonly IWidgetRepository _repository = Substitute.For<IWidgetRepository>();
    private readonly IRandomDataGenerator _dataGenerator = Substitute.For<IRandomDataGenerator>();

    private CreateWidgetCommandHandler CreateHandler() => new(_repository, _dataGenerator, TestMapper.Instance);

    [Fact]
    public async Task Handle_FirstWidget_AssignsFirstGridCell()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)null);
        _dataGenerator.GenerateChartDataJson().Returns("{\"points\":[]}");

        var result = await CreateHandler().Handle(new CreateWidgetCommand("LineChart"), CancellationToken.None);

        Assert.Equal(0, result.Row);
        Assert.Equal(0, result.Column);
        Assert.Equal(WidgetType.LineChart, result.Type);
    }

    [Fact]
    public async Task Handle_WithExistingWidgets_AssignsNextGridCell()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)1);
        _dataGenerator.GenerateChartDataJson().Returns("{\"points\":[]}");

        var result = await CreateHandler().Handle(new CreateWidgetCommand("BarChart"), CancellationToken.None);

        Assert.Equal(0, result.Row);
        Assert.Equal(2, result.Column);
    }

    [Fact]
    public async Task Handle_FourthWidget_WrapsToNextRow()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)2);
        _dataGenerator.GenerateChartDataJson().Returns("{\"points\":[]}");

        var result = await CreateHandler().Handle(new CreateWidgetCommand("LineChart"), CancellationToken.None);

        Assert.Equal(1, result.Row);
        Assert.Equal(0, result.Column);
    }

    [Fact]
    public async Task Handle_TextWidget_UsesEmptyTextPayload_AndDoesNotCallDataGenerator()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)null);

        var result = await CreateHandler().Handle(new CreateWidgetCommand("Text"), CancellationToken.None);

        Assert.Equal(WidgetType.Text, result.Type);
        _dataGenerator.DidNotReceive().GenerateChartDataJson();
    }

    [Theory]
    [InlineData("LineChart")]
    [InlineData("BarChart")]
    public async Task Handle_ChartWidget_UsesDataGenerator(string type)
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)null);
        _dataGenerator.GenerateChartDataJson().Returns("{\"points\":[{\"label\":\"A\",\"value\":1}]}");

        await CreateHandler().Handle(new CreateWidgetCommand(type), CancellationToken.None);

        _dataGenerator.Received(1).GenerateChartDataJson();
    }

    [Fact]
    public async Task Handle_NumericTypeString_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateHandler().Handle(new CreateWidgetCommand("1"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SaveLosesOrderRace_RetriesWithRecomputedOrder()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)0, (int?)1);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(
            _ => Task.FromException(new UniqueConstraintViolationException("duplicate Order")),
            _ => Task.CompletedTask);

        var result = await CreateHandler().Handle(new CreateWidgetCommand("Text"), CancellationToken.None);

        // First attempt read max 0 and tried Order 1; after losing the race the
        // handler re-read max 1 and persisted Order 2 (row 0, column 2).
        Assert.Equal(0, result.Row);
        Assert.Equal(2, result.Column);
        await _repository.Received(2).AddAsync(Arg.Any<Widget>(), Arg.Any<CancellationToken>());
        await _repository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EveryAttemptLosesOrderRace_ThrowsAfterMaxAttempts()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)0);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new UniqueConstraintViolationException("duplicate Order")));

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(
            () => CreateHandler().Handle(new CreateWidgetCommand("Text"), CancellationToken.None));

        await _repository.Received(CreateWidgetCommandHandler.MaxAttempts)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PersistsWidgetAndSavesChanges()
    {
        _repository.GetMaxOrderAsync(Arg.Any<CancellationToken>()).Returns((int?)null);

        await CreateHandler().Handle(new CreateWidgetCommand("Text"), CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Any<Widget>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
