namespace OrderService.Domain.Interfaces;

using OrderService.Domain.Entities;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetOrdersReadyForDeliveryAsync(CancellationToken cancellationToken = default);

    Task<Order?> GetOrderByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetActiveOrdersAsync(CancellationToken cancellationToken = default);
}