namespace OrderService.Domain.Enums;

/// <summary>
/// Enum for menu item availability status from CatalogService
/// Mirrors CatalogService.Domain.Enums.ItemAvailabilityStatus
/// </summary>
public enum ItemAvailabilityStatus
{
    Available = 1,
    OutOfStock = 2,
    Discontinued = 3
}
