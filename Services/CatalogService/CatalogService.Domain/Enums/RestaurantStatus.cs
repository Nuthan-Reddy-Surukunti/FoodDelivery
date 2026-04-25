namespace CatalogService.Domain.Enums;

/// <summary>
/// Represents the operational status of a restaurant.
/// Values must match QuickBite.Shared.Enums.RestaurantStatus for consistency
/// in event-driven architecture across all services.
/// </summary>
public enum RestaurantStatus
{
    /// <summary>
    /// Restaurant awaiting admin approval (initial state)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Restaurant has been approved and is active/operational
    /// </summary>
    Active = 1,

    /// <summary>
    /// Restaurant has been suspended or temporarily inactive
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Restaurant has been permanently closed or rejected
    /// </summary>
    Inactive = 3
}
