namespace OrderService.Application.Services;
using FoodDelivery.Shared.Events.Order;
using MassTransit;

using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class OrderStatusService : IOrderStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IPaymentRepository _paymentRepository;

    public OrderStatusService(IOrderRepository orderRepository, IPaymentRepository paymentRepository, IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", request.OrderId);
        
        order.OrderStatus = request.TargetStatus;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (request.TargetStatus == OrderStatus.RestaurantAccepted) 
            order.PreparationStartTime = DateTime.UtcNow;
        else if (request.TargetStatus == OrderStatus.ReadyForPickup) 
            order.PickupTime = DateTime.UtcNow;
        else if (request.TargetStatus == OrderStatus.Delivered) 
            order.DeliveryTime = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Publish order status changed event
        var oldStatus = order.OrderStatus.ToString();
        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            OldStatus = oldStatus,
            NewStatus = request.TargetStatus.ToString()
        }, cancellationToken);

        return OrderMappings.MapToDto(order);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(Guid orderId, bool forceByAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);
        
        if (!forceByAdmin && order.OrderStatus != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot cancel order in {order.OrderStatus} status.");
        
        order.OrderStatus = OrderStatus.CancelRequestedByCustomer;
        order.CancelRequestedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        if (order.PaymentId.HasValue)
        {
            var payment = await _paymentRepository.GetByIdAsync(order.PaymentId.Value, cancellationToken);
            if (payment != null && payment.PaymentStatus == PaymentStatus.Success)
            {
                payment.PaymentStatus = PaymentStatus.RefundInitiated;
                await _paymentRepository.UpdateAsync(payment, cancellationToken);
            }
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Publish order cancelled event
        await _publishEndpoint.Publish(new OrderCancelledEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            CancellationReason = "Customer requested cancellation",
            RefundAmount = order.TotalAmount
        }, cancellationToken);

        return OrderMappings.MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderTimelineEntryDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);

        var timeline = new List<OrderTimelineEntryDto>
        {
            new() { Status = OrderStatus.DraftCart, OccurredAt = order.CreatedAt, Label = "Order Created" }
        };

        if (order.CheckoutStartedAt.HasValue)
            timeline.Add(new() { Status = OrderStatus.CheckoutStarted, OccurredAt = order.CheckoutStartedAt.Value, Label = "Checkout Started" });

        if (order.PaymentCompletedAt.HasValue)
            timeline.Add(new() { Status = OrderStatus.Paid, OccurredAt = order.PaymentCompletedAt.Value, Label = "Payment Completed" });

        if (order.PreparationStartTime.HasValue)
            timeline.Add(new() { Status = OrderStatus.Preparing, OccurredAt = order.PreparationStartTime.Value, Label = "Preparing" });

        if (order.PickupTime.HasValue)
            timeline.Add(new() { Status = OrderStatus.PickedUp, OccurredAt = order.PickupTime.Value, Label = "Picked Up" });

        if (order.DeliveryTime.HasValue)
            timeline.Add(new() { Status = OrderStatus.Delivered, OccurredAt = order.DeliveryTime.Value, Label = "Delivered" });

        if (order.CancelRequestedAt.HasValue)
            timeline.Add(new() { Status = OrderStatus.CancelRequestedByCustomer, OccurredAt = order.CancelRequestedAt.Value, Label = "Cancel Requested" });

        return timeline.AsReadOnly();
    }

    public async Task<OrderDetailDto> SimulatePaymentAsync(SimulatePaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            throw new ValidationException("Only CashOnDelivery is currently supported.");
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", request.OrderId);

        var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken) ?? new Payment
        {
            OrderId = request.OrderId,
            CreatedAt = DateTime.UtcNow
        };

        payment.PaymentMethod = PaymentMethod.CashOnDelivery;
        payment.Amount = request.Amount ?? order.TotalAmount;
        payment.PaymentStatus = request.IsSuccessful ? PaymentStatus.Success : PaymentStatus.Failed;
        payment.TransactionId = request.TransactionId ?? Guid.NewGuid().ToString();
        payment.ProcessedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.FailureReason = request.IsSuccessful ? null : request.FailureReason;

        if (payment.Id == Guid.Empty)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
        }
        else
        {
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }

        if (request.IsSuccessful)
        {
            order.PaymentId = payment.Id;
            order.Payment = payment;
            order.PaymentCompletedAt = DateTime.UtcNow;
            order.OrderStatus = request.AutoAcceptByRestaurant ? OrderStatus.RestaurantAccepted : OrderStatus.Paid;
        }
        else
        {
            order.OrderStatus = OrderStatus.PaymentFailed;
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return OrderMappings.MapToDto(order);
    }
}
