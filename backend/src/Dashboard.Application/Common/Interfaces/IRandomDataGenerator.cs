namespace Dashboard.Application.Common.Interfaces;

public interface IRandomDataGenerator
{
    /// <summary>Returns a JSON payload of the shape {"points":[{"label":string,"value":number}, ...]}.</summary>
    string GenerateChartDataJson();
}
