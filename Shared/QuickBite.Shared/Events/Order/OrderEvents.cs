namespace QuickBite.Shared.Events.Order;

/// <summary>
/// Represents a snapshot of an order item for event publishing
/// </summary>
public class OrderItemSnapshot
{
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
}

/// <summary>
/// Published when a new order is placed
/// </summary>
public class OrderPlacedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public List<OrderItemSnapshot> Items { get; set; } = new();

    /// <summary>Payment method used: "CashOnDelivery" or "Online" (Razorpay)</summary>
    public string PaymentMethod { get; set; } = "CashOnDelivery";

    /// <summary>Internal Payment record ID — used by the Saga to trigger refunds</summary>
    public Guid? PaymentId { get; set; }
}

/// <summary>
/// Published when order status changes
/// </summary>
public class OrderStatusChangedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? StatusReason { get; set; }
}

/// <summary>
/// Published when order is cancelled
/// </summary>
public class OrderCancelledEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
}
