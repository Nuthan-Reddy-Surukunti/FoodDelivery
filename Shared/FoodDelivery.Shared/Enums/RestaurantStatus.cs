namespace FoodDelivery.Shared.Enums;

/// <summary>
/// Unified restaurant status definition used across all services
/// to ensure consistent state representation in event-driven architecture
/// </summary>
public enum RestaurantStatus
{
    /// <summary>
    /// Restaurant registration is pending admin approval (initial state)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Restaurant has been approved by admin and is active/operational
    /// </summary>
    Active = 1,

    /// <summary>
    /// Restaurant has been suspended or temporarily inactive
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Restaurant has been permanently removed or rejected
    /// </summary>
    Inactive = 3
}
