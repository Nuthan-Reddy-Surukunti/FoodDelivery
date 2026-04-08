using AdminService.Domain.Enums;
using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for Restaurant entity operations
/// </summary>
public interface IRestaurantRepository : IRepository<Restaurant>
{
    Task<IEnumerable<Restaurant>> GetByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Restaurant>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, RestaurantStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
