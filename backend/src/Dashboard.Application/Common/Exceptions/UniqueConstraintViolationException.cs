namespace Dashboard.Application.Common.Exceptions;

/// <summary>
/// Thrown by the persistence layer when saving changes violates a unique constraint
/// (e.g. two concurrent creates racing for the same widget Order). Callers may retry.
/// </summary>
public class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
