using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;
using Dashboard.Domain.Exceptions;
using Xunit;

namespace Dashboard.Domain.Tests;

public class WidgetTests
{
    [Fact]
    public void Create_SetsPropertiesFromArguments()
    {
        var widget = Widget.Create(WidgetType.Text, order: 2, dataJson: "{\"text\":\"hello\"}");

        Assert.Equal(WidgetType.Text, widget.Type);
        Assert.Equal(2, widget.Order);
        Assert.Equal("{\"text\":\"hello\"}", widget.DataJson);
    }

    [Fact]
    public void Create_WithEmptyDataJson_Throws()
    {
        Assert.Throws<ArgumentException>(() => Widget.Create(WidgetType.Text, order: 0, dataJson: ""));
    }

    [Fact]
    public void Create_WithNegativeOrder_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Widget.Create(WidgetType.Text, order: -1, dataJson: "{\"text\":\"x\"}"));
    }

    [Fact]
    public void UpdateText_OnTextWidget_UpdatesDataJson()
    {
        var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"old\"}");

        widget.UpdateText("{\"text\":\"new\"}");

        Assert.Equal("{\"text\":\"new\"}", widget.DataJson);
    }

    [Theory]
    [InlineData(WidgetType.LineChart)]
    [InlineData(WidgetType.BarChart)]
    public void UpdateText_OnNonTextWidget_ThrowsInvalidWidgetOperationException(WidgetType type)
    {
        var widget = Widget.Create(type, order: 0, dataJson: "{\"points\":[]}");

        Assert.Throws<InvalidWidgetOperationException>(() => widget.UpdateText("{\"text\":\"nope\"}"));
    }

    [Fact]
    public void UpdateText_WithEmptyDataJson_Throws()
    {
        var widget = Widget.Create(WidgetType.Text, order: 0, dataJson: "{\"text\":\"old\"}");

        Assert.Throws<ArgumentException>(() => widget.UpdateText(""));
    }
}
