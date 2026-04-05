namespace OrderService.Domain.Exceptions;

using OrderService.Domain.Enums;

public class OrderException : Exception
{
    public OrderException(string message)
        : base(message)
    {
    }
}

public sealed class InvalidOrderStatusTransitionException : OrderException
{
    public InvalidOrderStatusTransitionException(OrderStatus currentStatus, OrderStatus targetStatus)
        : base($"Invalid order status transition from '{currentStatus}' to '{targetStatus}'.")
    {
    }
}

public sealed class OrderCancellationNotAllowedException : OrderException
{
    public OrderCancellationNotAllowedException(OrderStatus currentStatus)
        : base($"Order cancellation is not allowed for status '{currentStatus}'.")
    {
    }
}

public sealed class InsufficientAddressDataException : OrderException
{
    public InsufficientAddressDataException()
        : base("Delivery address data is incomplete or invalid.")
    {
    }
}