using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

public interface IRestaurantRepository : IRepository<Restaurant>
{
    Task<IEnumerable<Restaurant>> GetByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Restaurant>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, RestaurantStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
