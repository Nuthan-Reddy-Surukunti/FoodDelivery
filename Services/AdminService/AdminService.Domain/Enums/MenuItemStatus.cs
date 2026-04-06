namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the status of a menu item
/// </summary>
public enum MenuItemStatus
{
    /// <summary>
    /// Menu item is active and available for ordering
    /// </summary>
    Active = 1,

    /// <summary>
    /// Menu item is inactive and not available for ordering
    /// </summary>
    Inactive = 2
}