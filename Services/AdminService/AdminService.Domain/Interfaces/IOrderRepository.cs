using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<int> GetTotalOrdersCountAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
    Task<int> GetOrdersCountByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetRevenueBetweenDatesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
