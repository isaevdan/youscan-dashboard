using Dashboard.Application.Widgets.Commands.CreateWidget;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Commands;

public class CreateWidgetCommandValidatorTests
{
    private readonly CreateWidgetCommandValidator _validator = new();

    [Theory]
    [InlineData("LineChart")]
    [InlineData("BarChart")]
    [InlineData("Text")]
    [InlineData("text")]
    public void Validate_WithValidType_HasNoErrors(string type)
    {
        var result = _validator.Validate(new CreateWidgetCommand(type));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("PieChart")]
    [InlineData("123")]
    public void Validate_WithInvalidType_HasErrors(string type)
    {
        var result = _validator.Validate(new CreateWidgetCommand(type));

        Assert.False(result.IsValid);
    }
}
