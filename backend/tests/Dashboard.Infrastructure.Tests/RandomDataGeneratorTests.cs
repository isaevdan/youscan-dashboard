using System.Text.Json;
using Dashboard.Infrastructure;
using Xunit;

namespace Dashboard.Infrastructure.Tests;

public class RandomDataGeneratorTests
{
    private readonly RandomDataGenerator _generator = new();

    [Fact]
    public void GenerateChartDataJson_ReturnsPointsWithLabelAndNumericValue()
    {
        var json = _generator.GenerateChartDataJson();

        using var doc = JsonDocument.Parse(json);
        var points = doc.RootElement.GetProperty("points");

        Assert.True(points.GetArrayLength() >= 6);
        foreach (var point in points.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.String, point.GetProperty("label").ValueKind);
            Assert.Equal(JsonValueKind.Number, point.GetProperty("value").ValueKind);
        }
    }

    [Fact]
    public void GenerateChartDataJson_SuccessiveCalls_ProduceDifferentData()
    {
        var first = _generator.GenerateChartDataJson();
        var second = _generator.GenerateChartDataJson();

        Assert.NotEqual(first, second);
    }
}
