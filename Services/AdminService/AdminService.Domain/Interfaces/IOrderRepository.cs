using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for Order entity operations
/// </summary>
public interface IOrderRepository : IRepository<object>
{
    Task<IEnumerable<object>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetDisputedOrdersAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<object> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetOrdersByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
}
