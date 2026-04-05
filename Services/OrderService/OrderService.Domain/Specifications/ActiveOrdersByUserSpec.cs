namespace OrderService.Domain.Specifications;

using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

public sealed class ActiveOrdersByUserSpec
{
    public Guid UserId { get; }

    public ActiveOrdersByUserSpec(Guid userId)
    {
        UserId = userId;
    }

    public bool IsSatisfiedBy(Order order)
    {
        return order.UserId == UserId &&
               order.OrderStatus is not OrderStatus.Delivered and not OrderStatus.Refunded;
    }
}