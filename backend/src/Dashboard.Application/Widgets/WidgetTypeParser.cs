using Dashboard.Domain.Enums;

namespace Dashboard.Application.Widgets;

/// <summary>
/// Single source of truth for parsing widget type strings from the API.
/// Matches enum <em>names</em> only (case-insensitive) — numeric strings like "1"
/// are rejected, unlike <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/>.
/// </summary>
public static class WidgetTypeParser
{
    public static bool TryParse(string? value, out WidgetType type)
    {
        foreach (var name in Enum.GetNames<WidgetType>())
        {
            if (string.Equals(name, value, StringComparison.OrdinalIgnoreCase))
            {
                type = Enum.Parse<WidgetType>(name);
                return true;
            }
        }

        type = default;
        return false;
    }

    public static WidgetType Parse(string value) =>
        TryParse(value, out var type)
            ? type
            : throw new ArgumentException($"'{value}' is not a valid widget type.", nameof(value));
}
