namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Helpers;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class OrderStatusService : IOrderStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;

    public OrderStatusService(IOrderRepository orderRepository, IPaymentRepository paymentRepository, IDeliveryAssignmentRepository deliveryAssignmentRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _deliveryAssignmentRepository = deliveryAssignmentRepository ?? throw new ArgumentNullException(nameof(deliveryAssignmentRepository));
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", request.OrderId);
        
        order.OrderStatus = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (request.NewStatus == OrderStatus.Accepted) order.PreparationStartTime = DateTime.UtcNow;
        else if (request.NewStatus == OrderStatus.ReadyForPickup) order.PickupTime = DateTime.UtcNow;
        else if (request.NewStatus == OrderStatus.Delivered) order.DeliveryTime = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order, cancellationToken);
        return OrderMappings.MapToDto(order);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(Guid orderId, bool forceByAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);
        
        if (!forceByAdmin && order.OrderStatus != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel order in {order.OrderStatus} status.");
        
        order.OrderStatus = OrderStatus.Cancelled;
        order.CancelRequestedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (order.Payment is not null && order.Payment.PaymentStatus == PaymentStatus.Success)
            await InitiateRefundAsync(order, order.TotalAmount, cancellationToken);
        
        await _orderRepository.UpdateAsync(order, cancellationToken);
        return OrderMappings.MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderTimelineEntryDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);
        
        var timeline = new List<OrderTimelineEntryDto>();
        AddEntry(timeline, "Order Created", order.CreatedAt);
        AddEntry(timeline, "Checkout Started", order.CheckoutStartedAt);
        AddEntry(timeline, "Payment Completed", order.PaymentCompletedAt);
        AddEntry(timeline, "Preparation Started", order.PreparationStartTime);
        AddEntry(timeline, "Ready for Pickup", order.PickupTime);
        AddEntry(timeline, "Delivered", order.DeliveryTime);
        return timeline.AsReadOnly();
    }

    public async Task<OrderDetailDto> SimulatePaymentAsync(SimulatePaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", request.OrderId);
        
        var payment = order.Payment ?? new Payment { OrderId = order.Id, CreatedAt = DateTime.UtcNow };
        
        payment.PaymentMethod = request.PaymentMethod;
        payment.PaymentStatus = request.IsSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
        payment.Amount = request.Amount;
        payment.TransactionId = Guid.NewGuid().ToString();
        payment.ProcessedAt = DateTime.UtcNow;
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
        
        order.OrderStatus = request.IsSuccess ? OrderStatus.Accepted : OrderStatus.PaymentFailed;
        order.PaymentCompletedAt = request.IsSuccess ? DateTime.UtcNow : null;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        
        return OrderMappings.MapToDto(order);
    }

    private async Task InitiateRefundAsync(Order order, decimal refundAmount, CancellationToken cancellationToken)
    {
        if (order.Payment is null) return;
        order.Payment.RefundedAmount = refundAmount;
        order.Payment.PaymentStatus = PaymentStatus.Refunded;
        order.Payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(order.Payment, cancellationToken);
    }

    private static void AddEntry(List<OrderTimelineEntryDto> timeline, string @event, DateTime? timestamp)
    {
        if (timestamp.HasValue)
            timeline.Add(new OrderTimelineEntryDto { Event = @event, Timestamp = timestamp.Value, Details = @event });
    }
}
