using Dashboard.Application.Widgets.Queries.GetWidgets;
using Xunit;

namespace Dashboard.Application.Tests.Widgets.Queries;

public class GetWidgetsQueryValidatorTests
{
    private readonly GetWidgetsQueryValidator _validator = new();

    [Fact]
    public void Validate_DefaultLimit_HasNoErrors()
    {
        var result = _validator.Validate(new GetWidgetsQuery(After: null, Limit: 30));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_LimitOutsideAllowedRange_HasErrors(int limit)
    {
        var result = _validator.Validate(new GetWidgetsQuery(After: null, Limit: limit));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NegativeAfterCursor_HasErrors()
    {
        var result = _validator.Validate(new GetWidgetsQuery(After: -1, Limit: 30));

        Assert.False(result.IsValid);
    }
}
