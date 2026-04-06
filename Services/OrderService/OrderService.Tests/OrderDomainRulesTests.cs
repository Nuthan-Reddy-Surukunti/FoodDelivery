namespace OrderService.Tests;

using OrderService.Domain.Common;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

public class OrderDomainRulesTests
{
    [Test]
    public void OrderStatus_DefaultShouldBeDraftCart()
    {
        var order = new Order { UserId = Guid.NewGuid(), RestaurantId = Guid.NewGuid() };
        Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.DraftCart));
    }

    [Test]
    public void OrderStatusTransitionPolicy_ShouldRejectInvalidTransition()
    {
        Assert.That(OrderStatusTransitionPolicy.CanTransition(OrderStatus.DraftCart, OrderStatus.Paid), Is.False);
    }

    [Test]
    public void OrderStatusTransitionPolicy_ShouldAllowValidTransition()
    {
        Assert.That(OrderStatusTransitionPolicy.CanTransition(OrderStatus.DraftCart, OrderStatus.CheckoutStarted), Is.True);
    }

    [Test]
    public void CartItem_ShouldCalculateSubtotalCorrectly()
    {
        var item = new CartItem { Quantity = 2, PriceSnapshot = 99.50m };
        Assert.That(item.Subtotal, Is.EqualTo(199.00m));
    }

    [Test]
    public void OrderItem_ShouldCalculateSubtotalCorrectly()
    {
        var item = new OrderItem { Quantity = 3, UnitPriceSnapshot = 100m };
        Assert.That(item.Subtotal, Is.EqualTo(300m));
    }

    [Test]
    public void Cart_DefaultStatusShouldBeActive()
    {
        var cart = new Cart { UserId = Guid.NewGuid(), RestaurantId = Guid.NewGuid() };
        Assert.That(cart.Status, Is.EqualTo(CartStatus.Active));
    }

    [Test]
    public void Payment_DefaultStatusShouldBePending()
    {
        var payment = new Payment { OrderId = Guid.NewGuid(), Amount = 100m };
        Assert.That(payment.PaymentStatus, Is.EqualTo(PaymentStatus.Pending));
    }

    [Test]
    public void DeliveryAssignment_DefaultStatusShouldBePickupPending()
    {
        var assignment = new DeliveryAssignment { OrderId = Guid.NewGuid(), DeliveryAgentId = Guid.NewGuid(), AssignedAt = DateTime.UtcNow };
        Assert.That(assignment.CurrentStatus, Is.EqualTo(DeliveryStatus.PickupPending));
    }
}