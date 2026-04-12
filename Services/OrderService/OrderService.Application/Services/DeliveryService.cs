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
    private const int MaxActiveAssignmentsPerAgent = 5;

    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDeliveryAgentRepository _deliveryAgentRepository;
    private readonly DeliveryEmailOptions _emailOptions;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(
        IOrderRepository orderRepository,
        IDeliveryAssignmentRepository deliveryAssignmentRepository,
        IPaymentRepository paymentRepository,
        IDeliveryAgentRepository deliveryAgentRepository,
        IOptions<DeliveryEmailOptions> emailOptions,
        ILogger<DeliveryService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _deliveryAssignmentRepository = deliveryAssignmentRepository ?? throw new ArgumentNullException(nameof(deliveryAssignmentRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _deliveryAgentRepository = deliveryAgentRepository ?? throw new ArgumentNullException(nameof(deliveryAgentRepository));
        _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        var active = assignments.Where(a => a.CurrentStatus != DeliveryStatus.Delivered).ToList();
        return active.Select(assignment => MapToDto(assignment)).ToList().AsReadOnly();
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            throw new ValidationException("Only CashOnDelivery is currently supported.");
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null) throw new ResourceNotFoundException("Order", orderId);

        var existingAssignment = await _deliveryAssignmentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        DeliveryAgent? assignedAgent = null;

        if (existingAssignment is null)
        {
            assignedAgent = await SelectAvailableAgentAsync(cancellationToken);
            if (assignedAgent is null)
            {
                throw new ValidationException("Payment cannot be processed because no delivery agents are currently available.");
            }
        }
        else
        {
            assignedAgent = await _deliveryAgentRepository.GetByIdAsync(existingAssignment.DeliveryAgentId, cancellationToken);
        }

        // Fetch or create payment
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        var isNewPayment = existingPayment == null;
        
        var payment = existingPayment ?? new Payment
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

        // Add or update based on whether it's new
        if (isNewPayment)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
        }
        else
        {
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }

        if (existingAssignment is null)
        {
            existingAssignment = new DeliveryAssignment
            {
                OrderId = order.Id,
                DeliveryAgentId = assignedAgent!.Id,
                AssignedAt = DateTime.UtcNow,
                CurrentStatus = DeliveryStatus.PickupPending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _deliveryAssignmentRepository.AddAsync(existingAssignment, cancellationToken);

            await NotifyAssignedAgentAsync(assignedAgent, order, existingAssignment, cancellationToken);

            _logger.LogInformation(
                "Delivery assigned for order {OrderId} to agent {AgentId} ({AgentName})",
                order.Id, assignedAgent.Id, assignedAgent.FullName);
        }

        order.PaymentId = payment.Id;
        order.Payment = payment;
        order.DeliveryAssignmentId = existingAssignment.Id;
        order.DeliveryAssignment = existingAssignment;
        order.PaymentCompletedAt = DateTime.UtcNow;
        order.OrderStatus = OrderStatus.OutForDelivery;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        
        var assignmentDto = MapToDto(existingAssignment, assignedAgent);
        
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
            var activeCount = assignments.Count(a => a.CurrentStatus != DeliveryStatus.Delivered);

            // Check if agent is below max capacity and has the fewest active assignments so far
            if (activeCount < MaxActiveAssignmentsPerAgent && activeCount < bestLoad)
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
               "- FoodDelivery Order Service";
    }
}
