namespace OrderService.Application.Interfaces;

using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;

public interface IOrderPlacementService
{
    Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDetailDto>> GetOrdersByUserAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDetailDto>> GetOrderQueueAsync(CancellationToken cancellationToken = default);

    Task<OrderDetailDto> ReorderFromHistoryAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<PartnerStatsDto> GetPartnerStatsAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}

