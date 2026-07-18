using System.Text.Json;
using Dashboard.Domain.Enums;

namespace Dashboard.Application.Widgets;

public sealed record WidgetDto(int Id, WidgetType Type, int Row, int Column, JsonElement Data);
