using AdminService.Domain.Enums;
using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for Order entity operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<int> GetTotalOrdersCountAsync(CancellationToken cancellationToken = default);
    Task<(decimal Amount, string Currency)> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
    Task<int> GetOrdersCountByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<(decimal Amount, string Currency)> GetRevenueBetweenDatesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
