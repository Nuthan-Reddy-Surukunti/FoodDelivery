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
using MassTransit;
using QuickBite.Shared.Events.Order;

public class DeliveryService : IDeliveryService
{
    // Capacity limit removed per user request for testing flexibility

    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDeliveryAgentRepository _deliveryAgentRepository;
    private readonly DeliveryEmailOptions _emailOptions;
    private readonly ILogger<DeliveryService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeliveryService(
        IOrderRepository orderRepository,
        IDeliveryAssignmentRepository deliveryAssignmentRepository,
        IPaymentRepository paymentRepository,
        IDeliveryAgentRepository deliveryAgentRepository,
        IOptions<DeliveryEmailOptions> emailOptions,
        ILogger<DeliveryService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _deliveryAssignmentRepository = deliveryAssignmentRepository ?? throw new ArgumentNullException(nameof(deliveryAssignmentRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _deliveryAgentRepository = deliveryAgentRepository ?? throw new ArgumentNullException(nameof(deliveryAgentRepository));
        _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task<IReadOnlyList<DeliveryAssignmentDto>> GetAssignedDeliveriesAsync(string authUserId, CancellationToken cancellationToken = default)
    {
        // Look up delivery agent by AuthUserId (from JWT token)
        var agent = await _deliveryAgentRepository.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (agent is null)
        {
            _logger.LogWarning("No delivery agent found for AuthUserId: {AuthUserId}", authUserId);
            return new List<DeliveryAssignmentDto>().AsReadOnly();
        }

        var assignments = await _deliveryAssignmentRepository.GetAssignmentsByAgentIdAsync(agent.Id, cancellationToken);
        
        // Only show the agent orders that are actionable (ReadyForPickup or beyond).
        // Orders still being prepared by the restaurant are not yet visible to the agent.
        var actionableAssignments = new List<DeliveryAssignment>();
        foreach (var assignment in assignments)
        {
            if (assignment.CurrentStatus == DeliveryStatus.Delivered) continue;
            
            var order = await _orderRepository.GetByIdAsync(assignment.OrderId, cancellationToken);
            if (order is null) continue;
            
            // Agent sees the order once it's ReadyForPickup, PickedUp, OutForDelivery, or Delivered
            if (order.OrderStatus >= OrderService.Domain.Enums.OrderStatus.ReadyForPickup)
            {
                actionableAssignments.Add(assignment);
            }
        }
        
        return actionableAssignments.Select(assignment => MapToDto(assignment)).ToList().AsReadOnly();
    }

    public async Task<AgentEarningsSummaryDto> GetEarningsSummaryAsync(string authUserId, CancellationToken cancellationToken = default)
    {
        var agent = await _deliveryAgentRepository.GetByAuthUserIdAsync(authUserId, cancellationToken);
        if (agent is null)
            return new AgentEarningsSummaryDto();

        var assignments = await _deliveryAssignmentRepository.GetAssignmentsByAgentIdAsync(agent.Id, cancellationToken);
        var delivered = assignments.Where(a => a.CurrentStatus == DeliveryStatus.Delivered).ToList();

        var today = DateTime.UtcNow.Date;
        var history = new List<AgentDeliveryRecordDto>();
        decimal totalValue = 0;
        decimal todayValue = 0;
        int todayCount = 0;

        foreach (var assignment in delivered)
        {
            var order = await _orderRepository.GetOrderByIdWithItemsAsync(assignment.OrderId, cancellationToken);
            if (order is null) continue;

            var orderTotal = order.TotalAmount;
            totalValue += orderTotal;

            var deliveredDate = assignment.DeliveredAt?.Date ?? assignment.AssignedAt.Date;
            if (deliveredDate == today)
            {
                todayValue += orderTotal;
                todayCount++;
            }

            history.Add(new AgentDeliveryRecordDto
            {
                OrderId = order.Id,
                RestaurantId = order.RestaurantId.ToString(),
                OrderTotal = orderTotal,
                ItemCount = order.OrderItems.Count,
                DeliveredAt = assignment.DeliveredAt,
            });
        }

        return new AgentEarningsSummaryDto
        {
            TotalDeliveries = delivered.Count,
            TodayDeliveries = todayCount,
            TotalOrderValue = totalValue,
            TodayOrderValue = todayValue,
            History = history,
        };
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);

        // Fetch or create payment
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        var isNewPayment = existingPayment == null;
        
        var payment = existingPayment ?? new Payment
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };

        payment.PaymentMethod = request.PaymentMethod;
        payment.Amount = request.Amount > 0 ? request.Amount : order.TotalAmount;
        payment.PaymentStatus = PaymentStatus.Success;  // All methods auto-succeed (simulated gateway)
        payment.ProcessedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.TransactionId ??= Guid.NewGuid().ToString();

        // Online payment handling — Razorpay details would be here if not CashOnDelivery
        if (request.PaymentMethod == PaymentMethod.Online)
        {
            payment.RazorpayOrderId = request.RazorpayOrderId;
            payment.RazorpayPaymentId = request.RazorpayPaymentId;
            payment.RazorpaySignature = request.RazorpaySignature;
        }

        // Add or update based on whether it's new
        if (isNewPayment)
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
        
        // ASSIGN DELIVERY AGENT (Now consolidated into a reusable method)
        var assignmentDto = await AssignDeliveryAgentAsync(orderId, cancellationToken);
        
        await _orderRepository.UpdateAsync(order, cancellationToken);
        
        // Publish OrderPlacedEvent - this notifies restaurants and other services
        await _publishEndpoint.Publish(new OrderPlacedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            RestaurantName = "Restaurant", // Can be updated if needed
            TotalAmount = order.TotalAmount,
            DeliveryAddress = $"{order.DeliveryAddressLine1}, {order.DeliveryCity}",
            Items = order.OrderItems.Select(oi => new OrderItemSnapshot
            {
                MenuItemId = oi.MenuItemId,
                MenuItemName = "Menu Item",
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.UnitPrice
            }).ToList()
        }, cancellationToken);

        // Publish OrderStatusChangedEvent for tracking consistency
        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            OldStatus = OrderStatus.CheckoutStarted.ToString(),
            NewStatus = OrderStatus.Paid.ToString()
        }, cancellationToken);
        
        return new PaymentResponseDto
        {
            PaymentId = payment.Id,
            PaymentStatus = PaymentStatus.Success,
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            Currency = "INR",
            PaymentMethod = payment.PaymentMethod,
            ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow,
            DeliveryAssignment = assignmentDto
        };
    }

    public async Task<DeliveryAssignmentDto> AssignDeliveryAgentAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);

        var existingAssignment = await _deliveryAssignmentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        DeliveryAgent? assignedAgent = null;

        if (existingAssignment is null)
        {
            assignedAgent = await SelectAvailableAgentAsync(cancellationToken);
            if (assignedAgent is null)
            {
                _logger.LogWarning("Assignment failed for order {OrderId}: No available agents.", orderId);
                // We don't throw here to allow payment to succeed even if assignment is delayed, 
                // but for this system, we want to ensure assignment happens.
                throw new ValidationException("No delivery agents are currently available to take this order.");
            }

            existingAssignment = new DeliveryAssignment
            {
                OrderId = order.Id,
                DeliveryAgentId = assignedAgent.Id,
                AssignedAt = DateTime.UtcNow,
                CurrentStatus = DeliveryStatus.PickupPending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _deliveryAssignmentRepository.AddAsync(existingAssignment, cancellationToken);

            // Update order with assignment link
            order.DeliveryAssignmentId = existingAssignment.Id;
            await _orderRepository.UpdateAsync(order, cancellationToken);

            await NotifyAssignedAgentAsync(assignedAgent, order, existingAssignment, cancellationToken);

            _logger.LogInformation(
                "Delivery assigned for order {OrderId} to agent {AgentId} ({AgentName})",
                order.Id, assignedAgent.Id, assignedAgent.FullName);
        }
        else
        {
            assignedAgent = await _deliveryAgentRepository.GetByIdAsync(existingAssignment.DeliveryAgentId, cancellationToken);
        }

        return MapToDto(existingAssignment, assignedAgent);
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

    private static DeliveryAssignmentDto MapToDto(DeliveryAssignment assignment, DeliveryAgent? agent = null) => new()
    {
        DeliveryAssignmentId = assignment.Id,
        DeliveryAgentId = assignment.DeliveryAgentId,
        OrderId = assignment.OrderId,
        AgentAuthUserId = agent?.AuthUserId ?? assignment.DeliveryAgent?.AuthUserId,
        AssignedAt = assignment.AssignedAt,
        PickedUpAt = assignment.PickedUpAt,
        DeliveredAt = assignment.DeliveredAt,
        CurrentStatus = assignment.CurrentStatus,
        AgentName = agent?.FullName ?? assignment.DeliveryAgent?.FullName,
        AgentEmail = agent?.Email ?? assignment.DeliveryAgent?.Email,
        AgentPhone = agent?.PhoneNumber ?? assignment.DeliveryAgent?.PhoneNumber
    };

    private async Task<DeliveryAgent?> SelectAvailableAgentAsync(CancellationToken cancellationToken)
    {
        // Get all active and verified delivery agents from database
        var agents = await _deliveryAgentRepository.GetAllActiveAndVerifiedAsync(cancellationToken);

        if (!agents.Any())
        {
            _logger.LogWarning("No active and verified delivery agents found");
            return null;
        }

        DeliveryAgent? selected = null;
        var bestLoad = int.MaxValue;

        // Load-balance: find agent with fewest active assignments
        foreach (var agent in agents)
        {
            var assignments = await _deliveryAssignmentRepository.GetAssignmentsByAgentIdAsync(agent.Id, cancellationToken);
            
            // AUTO-CLEANUP: Mark old stuck assignments from previous tests as Delivered to clear the queue
            // Any order older than 10 minutes that hasn't been finished is considered 'stale' for dev testing.
            var staleThreshold = DateTime.UtcNow.AddMinutes(-10);
            foreach (var stale in assignments.Where(a => a.CurrentStatus != DeliveryStatus.Delivered && a.CreatedAt < staleThreshold))
            {
                stale.CurrentStatus = DeliveryStatus.Delivered;
                stale.DeliveredAt = DateTime.UtcNow;
                await _deliveryAssignmentRepository.UpdateAsync(stale, cancellationToken);
            }

            // Recalculate active count after cleanup
            var activeCount = assignments.Count(a => a.CurrentStatus != DeliveryStatus.Delivered);

            // Find the agent with the lowest load (no upper limit check)
            if (activeCount < bestLoad)
            {
                bestLoad = activeCount;
                selected = agent;
            }
        }

        if (selected is null)
        {
            _logger.LogWarning(
                "No available delivery agents with capacity below {MaxCapacity}",
                MaxActiveAssignmentsPerAgent);
        }

        return selected;
    }

    private async Task NotifyAssignedAgentAsync(
        DeliveryAgent agent,
        Order order,
        DeliveryAssignment assignment,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(agent.Email))
        {
            _logger.LogWarning(
                "No email configured for assigned delivery agent {AgentId} ({AgentName})",
                agent.Id, agent.FullName);
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

        try
        {
            using var smtpClient = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
            {
                EnableSsl = _emailOptions.EnableSsl,
                Credentials = new NetworkCredential(_emailOptions.SenderEmail, _emailOptions.SenderPassword),
                Timeout = 10000  // 10 second timeout
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailOptions.SenderEmail),
                Subject = $"New Delivery Assignment - Order {order.Id}",
                Body = BuildDeliveryEmailBody(agent, order, assignment),
                IsBodyHtml = false
            };

            message.To.Add(agent.Email);
            
            // Send email asynchronously without cancellation token (SmtpClient.SendMailAsync doesn't support CancellationToken)
            await smtpClient.SendMailAsync(message);
            
            _logger.LogInformation(
                "Delivery assignment email sent successfully to agent {AgentEmail} for order {OrderId}",
                agent.Email, order.Id);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(
                smtpEx,
                "SMTP Error sending delivery assignment email to agent {AgentEmail} for order {OrderId}. Status: {StatusCode}",
                agent.Email, order.Id, smtpEx.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send delivery assignment email to agent {AgentEmail} for order {OrderId}. Error: {ErrorMessage}",
                agent.Email, order.Id, ex.Message);
        }
    }

    private static string BuildDeliveryEmailBody(DeliveryAgent agent, Order order, DeliveryAssignment assignment)
    {
        var address = string.Join(", ",
            new[] { order.DeliveryAddressLine1, order.DeliveryAddressLine2, order.DeliveryCity, order.DeliveryPostalCode }
                .Where(value => !string.IsNullOrWhiteSpace(value)));

        return $"Hello {agent.FullName},\n\n" +
               $"You have been assigned Order {order.Id}.\n" +
               $"Customer: {order.UserId}\n" +
               $"Assigned At: {assignment.AssignedAt:O}\n" +
               $"Deliver To: {address}\n\n" +
               "Please start pickup and update order status in the app.\n\n" +
               "- QuickBite Order Service";
    }
}
