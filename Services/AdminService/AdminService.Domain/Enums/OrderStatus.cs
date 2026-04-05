namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the current status of an order
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been placed but not confirmed
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Order has been confirmed by restaurant
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Order is being prepared
    /// </summary>
    Preparing = 3,

    /// <summary>
    /// Order is ready for pickup
    /// </summary>
    Ready = 4,

    /// <summary>
    /// Order is out for delivery
    /// </summary>
    OutForDelivery = 5,

    /// <summary>
    /// Order has been delivered
    /// </summary>
    Delivered = 6,

    /// <summary>
    /// Order was cancelled
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// Order has a dispute
    /// </summary>
    Disputed = 8
}
