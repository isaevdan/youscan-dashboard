using System.Text.Json;
using Dashboard.Domain.Entities;
using Dashboard.Domain.Enums;

namespace Dashboard.Application.Widgets;

public sealed record WidgetDto(int Id, WidgetType Type, int Row, int Column, JsonElement Data)
{
    public static WidgetDto FromEntity(Widget widget)
    {
        var row = widget.Order / Widget.ColumnsPerRow;
        var column = widget.Order % Widget.ColumnsPerRow;
        var data = JsonSerializer.Deserialize<JsonElement>(widget.DataJson);

        return new WidgetDto(widget.Id, widget.Type, row, column, data);
    }
}
