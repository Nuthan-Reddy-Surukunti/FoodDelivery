namespace OrderService.Application.Exceptions;

public class OrderApplicationException : Exception
{
    public OrderApplicationException(string message)
        : base(message)
    {
    }
}

public sealed class ResourceNotFoundException : OrderApplicationException
{
    public ResourceNotFoundException(string resourceName, Guid id)
        : base($"{resourceName} with ID '{id}' was not found.")
    {
    }
}

public sealed class ValidationException : OrderApplicationException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}