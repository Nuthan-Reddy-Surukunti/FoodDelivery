namespace OrderService.Application.Services;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Payment;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Application.Options;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class DeliveryService : IDeliveryService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly DeliveryAgentRoutingOptions _routingOptions;
    private readonly DeliveryEmailOptions _emailOptions;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(
        IOrderRepository orderRepository,
        IDeliveryAssignmentRepository deliveryAssignmentRepository,
        IPaymentRepository paymentRepository,
        IOptions<DeliveryAgentRoutingOptions> routingOptions,
        IOptions<DeliveryEmailOptions> emailOptions,
        ILogger<DeliveryService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _deliveryAssignmentRepository = deliveryAssignmentRepository ?? throw new ArgumentNullException(nameof(deliveryAssignmentRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _routingOptions = routingOptions?.Value ?? throw new ArgumentNullException(nameof(routingOptions));
        _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DeliveryAssignmentDto> AssignDeliveryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new ResourceNotFoundException("Order", orderId);
        }

        var existing = await _deliveryAssignmentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (existing is not null)
        {
            return MapToDto(existing);
        }

        var candidate = await SelectAvailableAgentAsync(cancellationToken)
            ?? throw new ValidationException("No delivery agents are currently available.");

        var assignment = new DeliveryAssignment
        {
            OrderId = order.Id,
            DeliveryAgentId = candidate.AgentId,
            AssignedAt = DateTime.UtcNow,
            CurrentStatus = DeliveryStatus.PickupPending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _deliveryAssignmentRepository.AddAsync(assignment, cancellationToken);

        order.DeliveryAssignmentId = assignment.Id;
        order.DeliveryAssignment = assignment;
        order.OrderStatus = OrderStatus.OutForDelivery;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        await NotifyAssignedAgentAsync(candidate, order, assignment, cancellationToken);

        return MapToDto(assignment);
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

        try
        {
            await AssignDeliveryAsync(orderId, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Auto delivery assignment skipped for order {OrderId}: {Message}", orderId, ex.Message);
        }
        
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

    private async Task<DeliveryAgentOption?> SelectAvailableAgentAsync(CancellationToken cancellationToken)
    {
        var activeAgents = _routingOptions.Agents
            .Where(agent => agent.IsActive && agent.AgentId != Guid.Empty)
            .ToList();

        if (!activeAgents.Any())
        {
            return null;
        }

        DeliveryAgentOption? selected = null;
        var bestLoad = int.MaxValue;

        foreach (var agent in activeAgents)
        {
            var assignments = await _deliveryAssignmentRepository.GetAssignmentsByAgentIdAsync(agent.AgentId, cancellationToken);
            var activeCount = assignments.Count(a => a.CurrentStatus != DeliveryStatus.Delivered);

            if (activeCount < _routingOptions.MaxActiveAssignmentsPerAgent && activeCount < bestLoad)
            {
                bestLoad = activeCount;
                selected = agent;
            }
        }

        return selected;
    }

    private async Task NotifyAssignedAgentAsync(
        DeliveryAgentOption agent,
        Order order,
        DeliveryAssignment assignment,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(agent.Email))
        {
            _logger.LogWarning("No email configured for assigned delivery agent {AgentId}", agent.AgentId);
            return;
        }

        if (!_emailOptions.Enabled)
        {
            _logger.LogInformation(
                "Delivery assignment email disabled. Agent: {AgentEmail}, Order: {OrderId}, Customer: {UserId}, Address: {Address}",
                agent.Email,
                order.Id,
                order.UserId,
                order.DeliveryAddressLine1 ?? string.Empty);
            return;
        }

        using var smtpClient = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            EnableSsl = _emailOptions.EnableSsl,
            Credentials = new NetworkCredential(_emailOptions.SenderEmail, _emailOptions.SenderPassword)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_emailOptions.SenderEmail),
            Subject = $"New Delivery Assignment - Order {order.Id}",
            Body = BuildDeliveryEmailBody(agent, order, assignment)
        };

        message.To.Add(agent.Email);
        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private static string BuildDeliveryEmailBody(DeliveryAgentOption agent, Order order, DeliveryAssignment assignment)
    {
        var address = string.Join(", ",
            new[] { order.DeliveryAddressLine1, order.DeliveryAddressLine2, order.DeliveryCity, order.DeliveryPostalCode }
                .Where(value => !string.IsNullOrWhiteSpace(value)));

        return $"Hello {agent.AgentName},\n\n" +
               $"You have been assigned Order {order.Id}.\n" +
               $"Customer: {order.UserId}\n" +
               $"Assigned At: {assignment.AssignedAt:O}\n" +
               $"Deliver To: {address}\n\n" +
               "Please start pickup and update order status in the app.\n\n" +
               "- FoodDelivery Order Service";
    }
}
