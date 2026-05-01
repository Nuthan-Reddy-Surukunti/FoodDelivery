namespace QuickBite.Shared.Events.Saga;

/// <summary>
/// Command published by the Saga to trigger order validation.
/// Consumed by ValidateOrderConsumer in OrderService.
/// </summary>
public class ValidateOrderCommand
{
    public Guid OrderId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<ValidateOrderItem> Items { get; set; } = new();
}

public class ValidateOrderItem
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Published by ValidateOrderConsumer when restaurant + all items are verified OK.
/// </summary>
public class OrderValidationSucceededEvent
{
    public Guid OrderId { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Published by ValidateOrderConsumer when restaurant is inactive or an item is out of stock.
/// </summary>
public class OrderValidationFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Command published by the Saga to trigger cancellation + optional refund.
/// Consumed by CompensateOrderConsumer in OrderService.
/// </summary>
public class CompensateOrderCommand
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;

    /// <summary>True when payment was Razorpay — triggers automatic refund.</summary>
    public bool ShouldRefund { get; set; }

    /// <summary>Internal PaymentId to look up Razorpay payment details for refund.</summary>
    public Guid? PaymentId { get; set; }

    public decimal Amount { get; set; }
}
