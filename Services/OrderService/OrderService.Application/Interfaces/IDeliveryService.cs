namespace OrderService.Application.Interfaces;

using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Payment;
using OrderService.Application.DTOs.Requests;

public interface IDeliveryService
{
    Task<DeliveryAssignmentDto> AssignDeliveryAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeliveryAssignmentDto>> GetAssignedDeliveriesAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default);

    Task<PaymentResponseDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderTimelineEntryDto>> GetDeliveryTimelineAsync(Guid deliveryAssignmentId, CancellationToken cancellationToken = default);
}
