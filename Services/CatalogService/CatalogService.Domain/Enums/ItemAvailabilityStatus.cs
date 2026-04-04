namespace CatalogService.Domain.Enums;

/// <summary>
/// Enum representing the availability status of a menu item.
/// </summary>
public enum ItemAvailabilityStatus
{
    /// <summary>Item is available for ordering.</summary>
    Available = 1,

    /// <summary>Item is temporarily unavailable (out of stock).</summary>
    OutOfStock = 2,

    /// <summary>Item is no longer offered (discontinued).</summary>
    Discontinued = 3
}
