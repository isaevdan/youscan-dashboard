using System.Text.Json;
using Dashboard.Api.Middleware;
using Dashboard.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dashboard.Api.Tests;

public class GlobalExceptionHandlerTests
{
    private sealed class CapturingLogger : ILogger<GlobalExceptionHandler>
    {
        public List<LogLevel> LoggedLevels { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
            => LoggedLevels.Add(logLevel);
    }

    private static async Task<(HttpContext Context, JsonElement Body)> HandleAsync(
        GlobalExceptionHandler handler, Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);

        context.Response.Body.Position = 0;
        var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        return (context, body);
    }

    [Fact]
    public async Task UniqueConstraintViolation_MapsTo409Conflict()
    {
        var handler = new GlobalExceptionHandler(new CapturingLogger());

        var (context, body) = await HandleAsync(
            handler, new UniqueConstraintViolationException("order race lost"));

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("Conflict", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task UniqueConstraintViolation_IsNotLoggedAsUnhandledError()
    {
        var logger = new CapturingLogger();
        var handler = new GlobalExceptionHandler(logger);

        await HandleAsync(handler, new UniqueConstraintViolationException("order race lost"));

        Assert.Empty(logger.LoggedLevels);
    }

    [Fact]
    public async Task UnknownException_MapsTo500AndLogsError()
    {
        var logger = new CapturingLogger();
        var handler = new GlobalExceptionHandler(logger);

        var (context, body) = await HandleAsync(handler, new InvalidOperationException("boom"));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("Server Error", body.GetProperty("title").GetString());
        Assert.Contains(LogLevel.Error, logger.LoggedLevels);
    }
}
