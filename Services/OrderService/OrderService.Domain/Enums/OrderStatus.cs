namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    DraftCart = 1,
    CheckoutStarted = 2,
    PaymentPending = 3,
    Paid = 4,
    RestaurantAccepted = 5,
    Preparing = 6,
    ReadyForPickup = 7,
    PickedUp = 8,
    OutForDelivery = 9,
    Delivered = 10,
    PaymentFailed = 11,
    CancelRequestedByCustomer = 12,
    RestaurantRejected = 13,
    RefundInitiated = 14,
    Refunded = 15
}