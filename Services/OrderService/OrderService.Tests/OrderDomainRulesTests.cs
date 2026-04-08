namespace OrderService.Tests;

using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

public class OrderDomainRulesTests
{
    [Test]
    public void Order_ShouldUseDraftStatusByDefault()
    {
        var order = new Order();

        Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.DraftCart));
    }

    [Test]
    public void Order_ShouldStoreBasicFieldsAndItems()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        var order = new Order
        {
            UserId = userId,
            RestaurantId = restaurantId,
            OrderStatus = OrderStatus.CheckoutStarted,
            DeliveryAddressLine1 = "123 Main St",
            DeliveryCity = "Bengaluru",
            DeliveryPostalCode = "560001",
            TotalAmount = 250m,
            OrderItems =
            [
                new OrderItem
                {
                    MenuItemId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100m,
                    Subtotal = 200m
                },
                new OrderItem
                {
                    MenuItemId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 50m,
                    Subtotal = 50m
                }
            ]
        };

        Assert.Multiple(() =>
        {
            Assert.That(order.UserId, Is.EqualTo(userId));
            Assert.That(order.RestaurantId, Is.EqualTo(restaurantId));
            Assert.That(order.OrderItems.Count, Is.EqualTo(2));
            Assert.That(order.TotalAmount, Is.EqualTo(250m));
            Assert.That(order.DeliveryCity, Is.EqualTo("Bengaluru"));
        });
    }

    [Test]
    public void Cart_ShouldHoldItemsAndStatus()
    {
        var cart = new Cart
        {
            UserId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            Status = CartStatus.Active,
            Items =
            [
                new CartItem
                {
                    MenuItemId = Guid.NewGuid(),
                    Quantity = 3,
                    Price = 80m,
                    Subtotal = 240m
                }
            ],
            TotalAmount = 240m
        };

        Assert.Multiple(() =>
        {
            Assert.That(cart.Status, Is.EqualTo(CartStatus.Active));
            Assert.That(cart.Items.Count, Is.EqualTo(1));
            Assert.That(cart.TotalAmount, Is.EqualTo(240m));
        });
    }

    [Test]
    public void Payment_ShouldStoreProcessingDetails()
    {
        var processedAt = DateTime.UtcNow;

        var payment = new Payment
        {
            OrderId = Guid.NewGuid(),
            Amount = 499m,
            PaymentMethod = PaymentMethod.Card,
            PaymentStatus = PaymentStatus.Success,
            TransactionId = "TXN-1001",
            ProcessedAt = processedAt
        };

        Assert.Multiple(() =>
        {
            Assert.That(payment.PaymentStatus, Is.EqualTo(PaymentStatus.Success));
            Assert.That(payment.TransactionId, Is.EqualTo("TXN-1001"));
            Assert.That(payment.ProcessedAt, Is.EqualTo(processedAt));
            Assert.That(payment.Amount, Is.EqualTo(499m));
        });
    }

    [Test]
    public void DeliveryAssignment_ShouldTrackStatusTimeline()
    {
        var assignment = new DeliveryAssignment
        {
            OrderId = Guid.NewGuid(),
            DeliveryAgentId = Guid.NewGuid(),
            AssignedAt = DateTime.UtcNow,
            CurrentStatus = DeliveryStatus.PickupPending
        };

        assignment.CurrentStatus = DeliveryStatus.PickedUp;
        assignment.PickedUpAt = assignment.AssignedAt.AddMinutes(12);
        assignment.CurrentStatus = DeliveryStatus.Delivered;
        assignment.DeliveredAt = assignment.PickedUpAt.Value.AddMinutes(22);

        Assert.Multiple(() =>
        {
            Assert.That(assignment.CurrentStatus, Is.EqualTo(DeliveryStatus.Delivered));
            Assert.That(assignment.PickedUpAt, Is.Not.Null);
            Assert.That(assignment.DeliveredAt, Is.Not.Null);
            Assert.That(assignment.DeliveredAt > assignment.PickedUpAt, Is.True);
        });
    }
}