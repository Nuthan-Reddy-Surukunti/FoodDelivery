using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Service interface for managing menu items with moderation capabilities
/// </summary>
public interface IMenuItemService
{
    /// <summary>
    /// Gets a menu item by ID
    /// </summary>
    Task<MenuItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets menu items with pagination and filters for admin use
    /// </summary>
    Task<PagedResultDto<MenuItemDto>> GetAllAsync(
        Guid? restaurantId = null,
        int pageNumber = 1, 
        int pageSize = 10, 
        string? status = null, 
        string? approvalStatus = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets menu items pending approval for moderation
    /// </summary>
    Task<IEnumerable<MenuItemDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new menu item
    /// </summary>
    Task<MenuItemDto> CreateAsync(CreateMenuItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing menu item
    /// </summary>
    Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a menu item (makes it available for ordering)
    /// </summary>
    Task<MenuItemDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a menu item (makes it unavailable for ordering)
    /// </summary>
    Task<MenuItemDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a menu item for content moderation
    /// </summary>
    Task<MenuItemDto> ApproveAsync(Guid id, ApproveMenuItemRequest request, string approvedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a menu item for content moderation
    /// </summary>
    Task<MenuItemDto> RejectAsync(Guid id, RejectMenuItemRequest request, string rejectedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets menu items for a specific restaurant (for restaurant use)
    /// </summary>
    Task<IEnumerable<MenuItemDto>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active menu items for a specific restaurant (for ordering)
    /// </summary>
    Task<IEnumerable<MenuItemDto>> GetActiveByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}