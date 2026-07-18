using Dashboard.Domain.Enums;
using Dashboard.Domain.Exceptions;

namespace Dashboard.Domain.Entities;

public class Widget
{
    /// <summary>Grid width. Row/Column for display are derived from <see cref="Order"/> as
    /// Order / ColumnsPerRow and Order % ColumnsPerRow.</summary>
    public const int ColumnsPerRow = 3;

    public int Id { get; private set; }
    public WidgetType Type { get; private set; }
    public int Order { get; private set; }
    public string DataJson { get; private set; } = string.Empty;

    private Widget()
    {
    }

    public static Widget Create(WidgetType type, int order, string dataJson)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be non-negative.");
        }

        if (string.IsNullOrWhiteSpace(dataJson))
        {
            throw new ArgumentException("Widget data must not be empty.", nameof(dataJson));
        }

        return new Widget
        {
            Type = type,
            Order = order,
            DataJson = dataJson
        };
    }

    public void UpdateText(string dataJson)
    {
        if (Type != WidgetType.Text)
        {
            throw new InvalidWidgetOperationException(
                $"Only {WidgetType.Text} widgets can be updated; this widget is {Type}.");
        }

        if (string.IsNullOrWhiteSpace(dataJson))
        {
            throw new ArgumentException("Widget data must not be empty.", nameof(dataJson));
        }

        DataJson = dataJson;
    }
}
