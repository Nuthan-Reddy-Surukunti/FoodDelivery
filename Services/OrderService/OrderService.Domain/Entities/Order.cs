namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Enums;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.DraftCart;
    public string? DeliveryStreet { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryPincode { get; set; }
    public AddressType? DeliveryAddressType { get; set; }
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public DateTime? CheckoutStartedAt { get; set; }
    public DateTime? PaymentCompletedAt { get; set; }
    public DateTime? PreparationStartTime { get; set; }
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime? CancelRequestedAt { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public Payment? Payment { get; set; }
    public DeliveryAssignment? DeliveryAssignment { get; set; }
}