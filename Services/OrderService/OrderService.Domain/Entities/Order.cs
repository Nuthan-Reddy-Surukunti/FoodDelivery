namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Constants;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _orderItems = [];

    public Guid UserId { get; private set; }

    public Guid RestaurantId { get; private set; }

    public OrderStatus OrderStatus { get; private set; } = OrderStatus.DraftCart;

    public Address? DeliveryAddress { get; private set; }

    public CouponCode? AppliedCoupon { get; private set; }

    public Payment? Payment { get; private set; }

    public DeliveryAssignment? DeliveryAssignment { get; private set; }

    public DateTime? CheckoutStartedAt { get; private set; }

    public DateTime? PaymentCompletedAt { get; private set; }

    public DateTime? PreparationStartTime { get; private set; }

    public DateTime? PickupTime { get; private set; }

    public DateTime? DeliveryTime { get; private set; }

    public DateTime? CancelRequestedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order()
    {
    }

    public Order(Guid userId, Guid restaurantId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        if (restaurantId == Guid.Empty)
        {
            throw new ArgumentException("Restaurant ID is required.", nameof(restaurantId));
        }

        UserId = userId;
        RestaurantId = restaurantId;
    }

    public void AddItem(Guid menuItemId, int quantity, decimal unitPriceSnapshot, string? customizationNotes = null)
    {
        EnsureMutableOrderItems();

        var existingItem = _orderItems.FirstOrDefault(item =>
            item.MenuItemId == menuItemId &&
            string.Equals(item.CustomizationNotes, customizationNotes, StringComparison.Ordinal));

        if (existingItem is not null)
        {
            throw new OrderException("Duplicate menu item entries are not allowed in order. Merge quantities before finalization.");
        }

        _orderItems.Add(new OrderItem(Id, menuItemId, quantity, unitPriceSnapshot, customizationNotes));
        Touch();
    }

    public void RemoveItem(Guid orderItemId)
    {
        EnsureMutableOrderItems();
        var removed = _orderItems.RemoveAll(item => item.Id == orderItemId) > 0;
        if (removed)
        {
            Touch();
        }
    }

    public void StartCheckout(Address deliveryAddress, DateTime atUtc)
    {
        if (deliveryAddress is null)
        {
            throw new InsufficientAddressDataException();
        }

        if (_orderItems.Count == 0)
        {
            throw new OrderException("Cannot start checkout for an empty order.");
        }

        DeliveryAddress = deliveryAddress;
        MoveToNextStatus(OrderStatus.CheckoutStarted, atUtc);
        CheckoutStartedAt = atUtc;
        Touch();
    }

    public void MarkPaymentPending(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.PaymentPending, atUtc);
    }

    public void AttachPayment(Payment payment)
    {
        if (payment.OrderId != Id)
        {
            throw new InvalidOperationException("Payment does not belong to this order.");
        }

        Payment = payment;
        Touch();
    }

    public void MarkPaid(DateTime atUtc)
    {
        if (Payment is null)
        {
            throw new InvalidOperationException("Payment record is required before marking order as paid.");
        }

        if (Payment.PaymentStatus != PaymentStatus.Success)
        {
            throw new InvalidOperationException("Order can be marked paid only when payment is successful.");
        }

        MoveToNextStatus(OrderStatus.Paid, atUtc);
        PaymentCompletedAt = atUtc;
        Touch();
    }

    public void AcceptByRestaurant(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.RestaurantAccepted, atUtc);
    }

    public void RejectByRestaurant(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.RestaurantRejected, atUtc);
    }

    public void StartPreparing(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.Preparing, atUtc);
        PreparationStartTime = atUtc;
        Touch();
    }

    public void MarkReadyForPickup(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.ReadyForPickup, atUtc);
    }

    public void MarkPickedUp(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.PickedUp, atUtc);
        PickupTime = atUtc;
        Touch();
    }

    public void MarkOutForDelivery(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.OutForDelivery, atUtc);
    }

    public void MarkDelivered(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.Delivered, atUtc);
        DeliveryTime = atUtc;
        Touch();
    }

    public void RequestCancellation(DateTime atUtc)
    {
        if (!CanCustomerCancel(atUtc))
        {
            throw new OrderCancellationNotAllowedException(OrderStatus);
        }

        MoveToNextStatus(OrderStatus.CancelRequestedByCustomer, atUtc);
        CancelRequestedAt = atUtc;
        Touch();
    }

    public void ForceCancelByAdmin(DateTime atUtc)
    {
        OrderStatus = OrderStatus.CancelRequestedByCustomer;
        CancelRequestedAt = atUtc;
        Touch();
    }

    public void InitiateRefund(DateTime atUtc)
    {
        if (!IsRefundEligible())
        {
            throw new InvalidOperationException("Order is not eligible for refund.");
        }

        MoveToNextStatus(OrderStatus.RefundInitiated, atUtc);
    }

    public void MarkRefunded(DateTime atUtc)
    {
        MoveToNextStatus(OrderStatus.Refunded, atUtc);
    }

    public bool CanCustomerCancel(DateTime atUtc)
    {
        if (PreparationStartTime.HasValue)
        {
            return false;
        }

        if (CreatedAt.AddMinutes(DomainConstants.CustomerCancellationWindowMinutes) < atUtc)
        {
            return false;
        }

        return OrderStatusTransitionPolicy.CanCustomerCancel(OrderStatus);
    }

    public bool IsRefundEligible()
    {
        if (!OrderStatusTransitionPolicy.IsRefundEligible(OrderStatus))
        {
            return false;
        }

        return Payment is not null &&
               (Payment.PaymentStatus == PaymentStatus.Success ||
                Payment.PaymentStatus == PaymentStatus.RefundInitiated ||
                Payment.PaymentStatus == PaymentStatus.Refunded);
    }

    public bool IsEligibleForDeliveryAssignment()
    {
        return DeliveryAssignment is null &&
               OrderStatusTransitionPolicy.IsEligibleForDeliveryAssignment(OrderStatus);
    }

    public void AssignDelivery(DeliveryAssignment assignment)
    {
        if (assignment.OrderId != Id)
        {
            throw new InvalidOperationException("Delivery assignment does not belong to this order.");
        }

        if (!IsEligibleForDeliveryAssignment())
        {
            throw new InvalidOperationException("Order is not eligible for delivery assignment.");
        }

        DeliveryAssignment = assignment;
        Touch();
    }

    public void ApplyCoupon(CouponCode couponCode, DateTime atUtc)
    {
        var subtotal = CalculateSubtotal();
        if (!couponCode.CanApplyTo(subtotal, RestaurantId, atUtc))
        {
            throw new InvalidOperationException("Coupon cannot be applied to this order.");
        }

        AppliedCoupon = couponCode;
        Touch();
    }

    public Money CalculateSubtotal(string currency = DomainConstants.DefaultCurrency)
    {
        var subtotal = _orderItems.Sum(item => item.Subtotal);
        return new Money(subtotal, currency);
    }

    public Money CalculateTax(decimal taxPercentage, string currency = DomainConstants.DefaultCurrency)
    {
        if (taxPercentage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taxPercentage), "Tax percentage cannot be negative.");
        }

        var subtotal = CalculateSubtotal(currency);
        var taxFactor = taxPercentage / 100;
        return subtotal.Multiply(taxFactor);
    }

    public Money CalculateTotal(decimal taxPercentage, string currency = DomainConstants.DefaultCurrency)
    {
        var subtotal = CalculateSubtotal(currency);
        var taxAmount = CalculateTax(taxPercentage, currency);
        var totalBeforeDiscount = subtotal.Add(taxAmount);

        if (AppliedCoupon is null)
        {
            return totalBeforeDiscount;
        }

        var discount = AppliedCoupon.CalculateDiscount(subtotal);
        if (discount.Amount > totalBeforeDiscount.Amount)
        {
            return Money.Zero(currency);
        }

        return totalBeforeDiscount.Subtract(discount);
    }

    public void MoveToNextStatus(OrderStatus nextStatus, DateTime atUtc)
    {
        if (!OrderStatusTransitionPolicy.CanTransition(OrderStatus, nextStatus))
        {
            throw new InvalidOrderStatusTransitionException(OrderStatus, nextStatus);
        }

        OrderStatus = nextStatus;

        switch (nextStatus)
        {
            case OrderStatus.CheckoutStarted:
                CheckoutStartedAt = atUtc;
                break;
            case OrderStatus.Paid:
                PaymentCompletedAt = atUtc;
                break;
            case OrderStatus.Preparing:
                PreparationStartTime = atUtc;
                break;
            case OrderStatus.PickedUp:
                PickupTime = atUtc;
                break;
            case OrderStatus.Delivered:
                DeliveryTime = atUtc;
                break;
            case OrderStatus.CancelRequestedByCustomer:
                CancelRequestedAt = atUtc;
                break;
        }

        Touch();
    }

    private void EnsureMutableOrderItems()
    {
        if (OrderStatus is not OrderStatus.DraftCart and not OrderStatus.CheckoutStarted)
        {
            throw new OrderException("Order items cannot be changed after payment flow begins.");
        }
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}