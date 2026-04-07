namespace OrderService.Application.Interfaces;

using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;

public interface IOrderStatusService
{
    Task<OrderDetailDto> UpdateOrderStatusAsync(UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> CancelOrderAsync(Guid orderId, bool forceByAdmin = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderTimelineEntryDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> SimulatePaymentAsync(SimulatePaymentRequestDto request, CancellationToken cancellationToken = default);
}
