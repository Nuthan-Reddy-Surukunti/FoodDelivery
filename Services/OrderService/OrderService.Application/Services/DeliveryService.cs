namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Payment;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class DeliveryService : IDeliveryService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DeliveryService(IOrderRepository orderRepository, IDeliveryAssignmentRepository deliveryAssignmentRepository, IPaymentRepository paymentRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _deliveryAssignmentRepository = deliveryAssignmentRepository ?? throw new ArgumentNullException(nameof(deliveryAssignmentRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    public async Task<IReadOnlyList<DeliveryAssignmentDto>> GetAssignedDeliveriesAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
    {
        var assignments = await _deliveryAssignmentRepository.GetAssignmentsByAgentIdAsync(deliveryAgentId, cancellationToken);
        var active = assignments.Where(a => a.CurrentStatus != DeliveryStatus.Delivered).ToList();
        return active.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            throw new ValidationException("Only CashOnDelivery is currently supported.");
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken) ?? new Payment
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };

        payment.PaymentMethod = PaymentMethod.CashOnDelivery;
        payment.Amount = request.Amount > 0 ? request.Amount : order.TotalAmount;
        payment.PaymentStatus = PaymentStatus.Success;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.TransactionId ??= Guid.NewGuid().ToString();

        if (payment.Id == Guid.Empty)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
        }
        else
        {
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }

        order.PaymentId = payment.Id;
        order.Payment = payment;
        order.PaymentCompletedAt = DateTime.UtcNow;
        order.OrderStatus = OrderStatus.Paid;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        
        return new PaymentResponseDto
        {
            PaymentId = payment.Id,
            PaymentStatus = PaymentStatus.Success,
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            Currency = "INR",
            PaymentMethod = payment.PaymentMethod,
            ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow
        };
    }

    public async Task<IReadOnlyList<OrderTimelineEntryDto>> GetDeliveryTimelineAsync(Guid deliveryAssignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = await _deliveryAssignmentRepository.GetByIdAsync(deliveryAssignmentId, cancellationToken);
        if (assignment is null) throw new ResourceNotFoundException("DeliveryAssignment", deliveryAssignmentId);

        var timeline = new List<OrderTimelineEntryDto>
        {
            new() { Status = OrderStatus.OutForDelivery, OccurredAt = assignment.CreatedAt, Label = "Delivery Started" }
        };

        if (assignment.PickedUpAt.HasValue)
            timeline.Add(new() { Status = OrderStatus.PickedUp, OccurredAt = assignment.PickedUpAt.Value, Label = "Picked Up" });

        if (assignment.DeliveredAt.HasValue)
            timeline.Add(new() { Status = OrderStatus.Delivered, OccurredAt = assignment.DeliveredAt.Value, Label = "Delivered" });

        return timeline.AsReadOnly();
    }

    private static DeliveryAssignmentDto MapToDto(DeliveryAssignment assignment) => new()
    {
        DeliveryAssignmentId = assignment.Id,
        DeliveryAgentId = assignment.DeliveryAgentId,
        AssignedAt = assignment.AssignedAt,
        PickedUpAt = assignment.PickedUpAt,
        DeliveredAt = assignment.DeliveredAt,
        CurrentStatus = assignment.CurrentStatus
    };
}
