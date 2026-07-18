using Dashboard.Application.Widgets.Commands.UpdateWidgetText;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Commands;

public class UpdateWidgetTextCommandValidatorTests
{
    private readonly UpdateWidgetTextCommandValidator _validator = new();

    [Fact]
    public void Validate_WithPositiveIdAndNonNullText_HasNoErrors()
    {
        var result = _validator.Validate(new UpdateWidgetTextCommand(1, "hello"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyText_HasNoErrors()
    {
        var result = _validator.Validate(new UpdateWidgetTextCommand(1, ""));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNonPositiveId_HasErrors()
    {
        var result = _validator.Validate(new UpdateWidgetTextCommand(0, "hello"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullText_HasErrors()
    {
        var result = _validator.Validate(new UpdateWidgetTextCommand(1, null!));

        Assert.False(result.IsValid);
    }
}
