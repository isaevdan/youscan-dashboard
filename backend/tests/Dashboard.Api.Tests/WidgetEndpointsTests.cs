using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dashboard.Application.Widgets.Commands.CreateWidget;
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

    private record WidgetsPageResponse(List<WidgetResponse> Items, bool HasMore, int? NextCursor);

    private async Task<WidgetsPageResponse> GetWidgetsAsync(int? after = null, int? limit = null)
    {
        var query = new List<string>();
        if (after is not null) query.Add($"after={after}");
        if (limit is not null) query.Add($"limit={limit}");
        var url = "/api/widgets" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WidgetsPageResponse>())!;
    }

    [Fact]
    public async Task GetWidgets_Initially_ReturnsEmptyPage()
    {
        var page = await GetWidgetsAsync();

        Assert.Empty(page.Items);
        Assert.False(page.HasMore);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task CreateWidget_ValidType_Returns201AndAppearsInList()
    {
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<WidgetResponse>();
        Assert.Equal("Text", created!.Type);

        var page = await GetWidgetsAsync();
        Assert.Contains(page.Items, w => w.Id == created.Id);
    }

    [Fact]
    public async Task CreateWidget_InvalidType_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "PieChart" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateWidget_NumericTypeString_Returns400()
    {
        // The contract is enum names only — "1" must not silently create a LineChart.
        var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateWidget_ParallelRequests_AssignDistinctGridPositions()
    {
        // Invariant: the parallel request count must be <= MaxAttempts — worst case a
        // request loses N-1 Order races before succeeding, so any count above
        // MaxAttempts could legitimately exhaust retries and return 409 instead of 201.
        // Referencing the constant keeps this test in lockstep with the handler.
        var responses = await Task.WhenAll(Enumerable.Range(0, CreateWidgetCommandHandler.MaxAttempts)
            .Select(_ => _client.PostAsJsonAsync("/api/widgets", new { type = "Text" })));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));

        var created = await Task.WhenAll(responses.Select(r => r.Content.ReadFromJsonAsync<WidgetResponse>()));
        var orders = created.Select(w => w!.Row * 3 + w.Column).ToList();

        Assert.Equal(orders.Count, orders.Distinct().Count());
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
    public async Task UpdateWidgetText_TextOverMaxLength_Returns400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        var created = await createResponse.Content.ReadFromJsonAsync<WidgetResponse>();

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/widgets/{created!.Id}", new { text = new string('a', 5001) });

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
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

        var page = await GetWidgetsAsync();
        Assert.DoesNotContain(page.Items, w => w.Id == created.Id);
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

    [Fact]
    public async Task GetWidgets_MoreWidgetsThanLimit_SetsHasMoreAndNextCursor()
    {
        for (var i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
        }

        var page = await GetWidgetsAsync(limit: 3);

        Assert.Equal(3, page.Items.Count);
        Assert.True(page.HasMore);
        Assert.NotNull(page.NextCursor);
    }

    [Fact]
    public async Task GetWidgets_SecondPageUsingCursor_ReturnsRemainingWidgetsWithNoOverlap()
    {
        var createdIds = new List<int>();
        for (var i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/widgets", new { type = "Text" });
            var created = await response.Content.ReadFromJsonAsync<WidgetResponse>();
            createdIds.Add(created!.Id);
        }

        var firstPage = await GetWidgetsAsync(limit: 3);
        var secondPage = await GetWidgetsAsync(after: firstPage.NextCursor, limit: 3);

        Assert.Equal(2, secondPage.Items.Count);
        Assert.False(secondPage.HasMore);
        Assert.Empty(firstPage.Items.Select(w => w.Id).Intersect(secondPage.Items.Select(w => w.Id)));
        Assert.Equal(createdIds, firstPage.Items.Select(w => w.Id).Concat(secondPage.Items.Select(w => w.Id)));
    }

    [Fact]
    public async Task GetWidgets_LimitTooHigh_Returns400()
    {
        var response = await _client.GetAsync("/api/widgets?limit=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiDocument_Returns200()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
