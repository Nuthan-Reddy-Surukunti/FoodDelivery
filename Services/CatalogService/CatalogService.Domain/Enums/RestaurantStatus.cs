namespace CatalogService.Domain.Enums;

/// <summary>
/// Enum representing the status/state of a restaurant.
/// </summary>
public enum RestaurantStatus
{
    /// <summary>Restaurant is open and accepting orders.</summary>
    Active = 1,

    /// <summary>Restaurant exists but is not accepting orders.</summary>
    Inactive = 2,

    /// <summary>Restaurant is temporarily suspended (e.g., health violation, payment issue).</summary>
    Suspended = 3
}
