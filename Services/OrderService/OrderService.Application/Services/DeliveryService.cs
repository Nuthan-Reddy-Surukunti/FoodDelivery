namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Payment;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Helpers;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class DeliveryService : IDeliveryService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;

    public DeliveryService(IOrderRepository orderRepository, IPaymentRepository paymentRepository, IDeliveryAssignmentRepository deliveryAssignmentRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _deliveryAssignmentRepository = deliveryAssignmentRepository ?? throw new ArgumentNullException(nameof(deliveryAssignmentRepository));
    }

    public async Task<IReadOnlyList<DeliveryAssignmentDto>> GetAssignedDeliveriesAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
    {
        var assignments = await _deliveryAssignmentRepository.GetByDeliveryAgentAsync(deliveryAgentId, cancellationToken);
        var active = assignments.Where(a => a.CurrentStatus != DeliveryStatus.Delivered && a.CurrentStatus != DeliveryStatus.Cancelled).ToList();
        return active.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);
        
        var payment = order.Payment ?? new Payment { OrderId = orderId, CreatedAt = DateTime.UtcNow };
        payment.PaymentMethod = request.PaymentMethod;
        payment.Amount = request.Amount;
        payment.PaymentStatus = PaymentStatus.Success;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.TransactionId = Guid.NewGuid().ToString();
        payment.UpdatedAt = DateTime.UtcNow;
        
        if (order.Payment is null)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
            order.Payment = payment;
            order.PaymentId = payment.Id;
        }
        else
        {
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }
        
        order.PaymentCompletedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        
        return new PaymentResponseDto
        {
            TransactionId = payment.TransactionId,
            PaymentStatus = "Success",
            Amount = payment.Amount,
            ProcessedAt = payment.ProcessedAt,
            Message = "Payment processed successfully"
        };
    }

    public async Task<IReadOnlyList<OrderTimelineEntryDto>> GetDeliveryTimelineAsync(Guid deliveryAssignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = await _deliveryAssignmentRepository.GetByIdAsync(deliveryAssignmentId, cancellationToken);
        if (assignment is null) throw new ResourceNotFoundException("DeliveryAssignment", deliveryAssignmentId);
        
        var timeline = new List<OrderTimelineEntryDto>();
        timeline.Add(new OrderTimelineEntryDto { Event = "Assigned", Timestamp = assignment.AssignedAt, Details = "Delivery assigned" });
        if (assignment.PickedUpAt.HasValue)
            timeline.Add(new OrderTimelineEntryDto { Event = "Picked Up", Timestamp = assignment.PickedUpAt.Value, Details = "Picked up" });
        if (assignment.DeliveredAt.HasValue)
            timeline.Add(new OrderTimelineEntryDto { Event = "Delivered", Timestamp = assignment.DeliveredAt.Value, Details = "Delivered" });
        return timeline.AsReadOnly();
    }

    private static DeliveryAssignmentDto MapToDto(DeliveryAssignment a) =>
        new() { DeliveryAssignmentId = a.Id, DeliveryAgentId = a.DeliveryAgentId, AssignedAt = a.AssignedAt, PickedUpAt = a.PickedUpAt, DeliveredAt = a.DeliveredAt, CurrentStatus = a.CurrentStatus };
}
