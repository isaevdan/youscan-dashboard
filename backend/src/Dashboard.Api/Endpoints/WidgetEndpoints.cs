using Dashboard.Application.Widgets.Commands.CreateWidget;
using Dashboard.Application.Widgets.Commands.DeleteWidget;
using Dashboard.Application.Widgets.Commands.UpdateWidgetText;
using Dashboard.Application.Widgets.Queries.GetWidgets;
using MediatR;

namespace Dashboard.Api.Endpoints;

public static class WidgetEndpoints
{
    public static IEndpointRouteBuilder MapWidgetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/widgets");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetWidgetsQuery(), ct)));

        group.MapPost("/", async (CreateWidgetRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateWidgetCommand(request.Type), ct);
            return Results.Created($"/api/widgets/{result.Id}", result);
        });

        group.MapPut("/{id:int}", async (int id, UpdateWidgetTextRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateWidgetTextCommand(id, request.Text), ct);
            return Results.Ok(result);
        });

        group.MapDelete("/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteWidgetCommand(id), ct);
            return Results.NoContent();
        });

        return app;
    }
}

public record CreateWidgetRequest(string Type);

public record UpdateWidgetTextRequest(string Text);
