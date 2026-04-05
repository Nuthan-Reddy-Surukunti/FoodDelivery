namespace OrderService.Domain.Specifications;

using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

public sealed class OrdersReadyForDeliverySpec
{
    public bool IsSatisfiedBy(Order order)
    {
        return order.OrderStatus == OrderStatus.ReadyForPickup;
    }
}