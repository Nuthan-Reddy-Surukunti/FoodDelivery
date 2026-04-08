using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for MenuItem operations
/// </summary>
public interface IMenuItemRepository : IRepository<MenuItem>
{
    /// <summary>
    /// Gets paged menu items with optional filters
    /// </summary>
    Task<(IEnumerable<MenuItem> Items, int TotalCount)> GetPagedAsync(
        Guid? restaurantId = null, 
        int pageNumber = 1, 
        int pageSize = 10, 
        MenuItemStatus? status = null, 
        ApprovalStatus? approvalStatus = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all menu items for a specific restaurant
    /// </summary>
    Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active menu items for a specific restaurant (for ordering)
    /// </summary>
    Task<IEnumerable<MenuItem>> GetActiveByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets menu items pending approval
    /// </summary>
    Task<IEnumerable<MenuItem>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a menu item name already exists for a restaurant (case-insensitive)
    /// </summary>
    Task<bool> ExistsWithNameAsync(Guid restaurantId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}