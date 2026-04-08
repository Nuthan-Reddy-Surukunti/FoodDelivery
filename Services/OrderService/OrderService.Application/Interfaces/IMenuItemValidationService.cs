namespace OrderService.Application.Interfaces;

/// <summary>
/// Service for validating menu item existence and availability
/// by calling CatalogService HTTP API
/// </summary>
public interface IMenuItemValidationService
{
    /// <summary>
    /// Validates that a menu item exists and is available in the catalog
    /// </summary>
    /// <param name="restaurantId">Restaurant ID</param>
    /// <param name="menuItemId">Menu Item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>MenuItemValidationResult with availability and current price info</returns>
    Task<MenuItemValidationResult> ValidateMenuItemAsync(
        Guid restaurantId,
        Guid menuItemId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of menu item validation
/// </summary>
public class MenuItemValidationResult
{
    /// <summary>
    /// Whether the item is valid and available
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Menu item ID
    /// </summary>
    public Guid MenuItemId { get; set; }

    /// <summary>
    /// Current price from catalog
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Item name
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Whether item is available for ordering
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
