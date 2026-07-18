namespace Dashboard.Domain.Exceptions;

public class InvalidWidgetOperationException : Exception
{
    public InvalidWidgetOperationException(string message) : base(message)
    {
    }
}
