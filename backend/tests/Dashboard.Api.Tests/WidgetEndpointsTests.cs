using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Dashboard.Api.Tests;

// Each test gets its own factory (and thus its own temp SQLite file) rather than
// sharing one via IClassFixture, since xUnit creates a new test-class instance per
// [Fact]; a shared fixture would let widget counts leak across tests.
public class WidgetEndpointsTests : IDisposable
{
    private readonly WidgetApiFactory _factory = new();
    private readonly HttpClient _client;

    public WidgetEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    private record WidgetResponse(int Id, string Type, int Row, int Column, JsonElement Data);

    [Fact]
    public async Task GetWidgets_Initially_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/widgets");

        response.EnsureSuccessStatusCode();
        var widgets = await response.Content.ReadFromJsonAsync<List<WidgetResponse>>();
        Assert.Empty(widgets!);
    }

    [Fact]
    public async Task CreateWidget_ValidType_Returns201AndAppearsInList()
    {
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<WidgetResponse>();
        Assert.Equal("Text", created!.Type);

        var listResponse = await _client.GetAsync("/api/widgets");
        var widgets = await listResponse.Content.ReadFromJsonAsync<List<WidgetResponse>>();
        Assert.Contains(widgets!, w => w.Id == created.Id);
    }

    [Fact]
    public async Task CreateWidget_InvalidType_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "PieChart" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetText_ExistingTextWidget_Returns200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        var created = await createResponse.Content.ReadFromJsonAsync<WidgetResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/widgets/{created!.Id}", new { text = "hello" });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<WidgetResponse>();
        Assert.Equal("hello", updated!.Data.GetProperty("text").GetString());
    }

    [Fact]
    public async Task UpdateWidgetText_MissingWidget_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/widgets/999999", new { text = "hello" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetText_OnChartWidget_Returns400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/widgets", new { type = "LineChart" });
        var created = await createResponse.Content.ReadFromJsonAsync<WidgetResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/widgets/{created!.Id}", new { text = "hello" });

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteWidget_Existing_Returns204AndRemovesFromList()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        var created = await createResponse.Content.ReadFromJsonAsync<WidgetResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/widgets/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/widgets");
        var widgets = await listResponse.Content.ReadFromJsonAsync<List<WidgetResponse>>();
        Assert.DoesNotContain(widgets!, w => w.Id == created.Id);
    }

    [Fact]
    public async Task DeleteWidget_Missing_Returns404()
    {
        var response = await _client.DeleteAsync("/api/widgets/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateWidget_ThirdInRow_HasRowZeroColumnTwo()
    {
        await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });

        var created = await response.Content.ReadFromJsonAsync<WidgetResponse>();

        Assert.Equal(0, created!.Row);
        Assert.Equal(2, created.Column);
    }

    [Fact]
    public async Task CreateWidget_FourthInRow_WrapsToRowOneColumnZero()
    {
        await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "LineChart" });

        var created = await response.Content.ReadFromJsonAsync<WidgetResponse>();

        Assert.Equal(1, created!.Row);
        Assert.Equal(0, created.Column);
    }
}
