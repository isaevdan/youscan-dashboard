using System.Text.Json;
using Dashboard.Application.Common.Interfaces;

namespace Dashboard.Infrastructure;

public class RandomDataGenerator : IRandomDataGenerator
{
    private static readonly string[] Labels =
        ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public string GenerateChartDataJson()
    {
        var points = Labels.Select(label => new
        {
            label,
            value = Random.Shared.Next(1, 101)
        });

        return JsonSerializer.Serialize(new { points });
    }
}
