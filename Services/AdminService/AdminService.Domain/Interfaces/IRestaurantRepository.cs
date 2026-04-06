using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for Restaurant entity operations
/// </summary>
public interface IRestaurantRepository : IRepository<object>
{
    Task<IEnumerable<object>> GetByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<object> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, RestaurantStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default);
    Task ApproveRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task RejectRestaurantAsync(Guid restaurantId, string reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
