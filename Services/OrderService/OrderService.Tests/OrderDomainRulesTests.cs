namespace OrderService.Tests;

using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

public class OrderDomainRulesTests
{
    [Test]
    public void MoveToNextStatus_ShouldRejectInvalidTransition()
    {
        var order = CreateDraftOrderWithItem();

        Assert.Throws<InvalidOrderStatusTransitionException>(() =>
            order.MoveToNextStatus(OrderStatus.Paid, DateTime.UtcNow));
    }

    [Test]
    public void RequestCancellation_ShouldFailAfterPreparingStarts()
    {
        var order = CreateReadyForPreparationOrder();

        order.StartPreparing(DateTime.UtcNow);

        Assert.That(order.CanCustomerCancel(DateTime.UtcNow), Is.False);
        Assert.Throws<OrderCancellationNotAllowedException>(() =>
            order.RequestCancellation(DateTime.UtcNow));
    }

    [Test]
    public void InitiateRefund_ShouldFailWhenAmountExceedsPayment()
    {
        var payment = new Payment(Guid.NewGuid(), new Money(499), PaymentMethod.Card);
        payment.MarkAsSuccess("TXN-1001", DateTime.UtcNow);

        Assert.Throws<InvalidRefundAmountException>(() =>
            payment.InitiateRefund(new Money(500), DateTime.UtcNow));
    }

    [Test]
    public void Cart_AddItem_ShouldRejectMixedRestaurantItems()
    {
        var primaryRestaurantId = Guid.NewGuid();
        var anotherRestaurantId = Guid.NewGuid();
        var cart = new Cart(Guid.NewGuid(), primaryRestaurantId);

        cart.AddItem(primaryRestaurantId, Guid.NewGuid(), 1, 99);

        Assert.Throws<MixedCartException>(() =>
            cart.AddItem(anotherRestaurantId, Guid.NewGuid(), 1, 120));
    }

    [Test]
    public void DeliveryAssignment_ShouldEnforceChronologicalTimestamps()
    {
        var assignedAt = DateTime.UtcNow;
        var assignment = new DeliveryAssignment(Guid.NewGuid(), Guid.NewGuid(), assignedAt);

        Assert.Throws<InvalidOperationException>(() =>
            assignment.MarkAsPickedUp(assignedAt.AddMinutes(-1)));

        var pickedUpAt = assignedAt.AddMinutes(10);
        assignment.MarkAsPickedUp(pickedUpAt);

        Assert.Throws<InvalidOperationException>(() =>
            assignment.MarkAsDelivered(pickedUpAt));
    }

    [Test]
    public void CalculateTotal_ShouldApplyTaxAndCouponDiscount()
    {
        var order = CreateDraftOrderWithItem();
        var coupon = new CouponCode(
            "SAVE10",
            10,
            new Money(100),
            DateTime.UtcNow.AddHours(2),
            order.RestaurantId);

        order.ApplyCoupon(coupon, DateTime.UtcNow);

        var total = order.CalculateTotal(5);

        Assert.That(total.Amount, Is.EqualTo(190));
    }

    [Test]
    public void IsEligibleForDeliveryAssignment_ShouldBeTrueOnlyWhenReadyForPickup()
    {
        var order = CreateReadyForPreparationOrder();

        Assert.That(order.IsEligibleForDeliveryAssignment(), Is.False);

        order.StartPreparing(DateTime.UtcNow.AddMinutes(1));
        order.MarkReadyForPickup(DateTime.UtcNow.AddMinutes(10));

        Assert.That(order.IsEligibleForDeliveryAssignment(), Is.True);
    }

    private static Order CreateDraftOrderWithItem()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 2, 100);
        return order;
    }

    private static Order CreateReadyForPreparationOrder()
    {
        var now = DateTime.UtcNow;
        var order = CreateDraftOrderWithItem();

        order.StartCheckout(CreateAddress(), now.AddMinutes(1));
        order.MarkPaymentPending(now.AddMinutes(2));

        var payment = new Payment(order.Id, new Money(200), PaymentMethod.Card);
        payment.MarkAsSuccess("TXN-READY", now.AddMinutes(3));

        order.AttachPayment(payment);
        order.MarkPaid(now.AddMinutes(3));
        order.AcceptByRestaurant(now.AddMinutes(4));

        return order;
    }

    private static Address CreateAddress()
    {
        return new Address("Street 12", "Hyderabad", "500081", AddressType.Home, 17.3850, 78.4867);
    }
}