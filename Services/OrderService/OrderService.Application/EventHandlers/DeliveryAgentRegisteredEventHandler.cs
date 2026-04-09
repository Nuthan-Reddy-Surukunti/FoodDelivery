using Microsoft.Extensions.Logging;
using FoodDelivery.Shared.Events.Auth;
using MassTransit;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.EventHandlers;

/// <summary>
/// Handles UserRegisteredEvent from AuthService via RabbitMQ.
/// Creates DeliveryAgent records in OrderService database when a delivery agent is registered in AuthService.
/// </summary>
public class DeliveryAgentRegisteredEventHandler : IConsumer<UserRegisteredEvent>
{
    private readonly IDeliveryAgentRepository _deliveryAgentRepository;
    private readonly ILogger<DeliveryAgentRegisteredEventHandler> _logger;

    public DeliveryAgentRegisteredEventHandler(
        IDeliveryAgentRepository deliveryAgentRepository,
        ILogger<DeliveryAgentRegisteredEventHandler> logger)
    {
        _deliveryAgentRepository = deliveryAgentRepository ?? throw new ArgumentNullException(nameof(deliveryAgentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes UserRegisteredEvent to sync delivery agents from AuthService to OrderService.
    /// Only creates records for users with Role="DeliveryAgent" and valid email addresses.
    /// </summary>
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        // Only process DeliveryAgent role registrations
        if (@event.Role != "DeliveryAgent")
        {
            _logger.LogDebug(
                "Skipping UserRegisteredEvent for non-DeliveryAgent role: UserId={UserId}, Role={Role}",
                @event.UserId, @event.Role);
            return;
        }

        // Validate email is not empty
        if (string.IsNullOrWhiteSpace(@event.Email))
        {
            _logger.LogWarning(
                "Cannot register delivery agent without email: UserId={UserId}, FullName={FullName}",
                @event.UserId, @event.FullName);
            return;
        }

        try
        {
            // Check if agent already exists to avoid duplicates
            var existingAgent = await _deliveryAgentRepository.GetByAuthUserIdAsync(@event.UserId.ToString());
            if (existingAgent != null)
            {
                _logger.LogDebug(
                    "DeliveryAgent already exists in database: AuthUserId={AuthUserId}, Email={Email}",
                    @event.UserId, @event.Email);
                return;
            }

            // Create new DeliveryAgent record
            var deliveryAgent = new DeliveryAgent
            {
                Id = Guid.NewGuid(),
                AuthUserId = @event.UserId.ToString(),
                FullName = @event.FullName,
                Email = @event.Email,
                IsActive = true,
                IsEmailVerified = true, // DeliveryAgents are auto-verified upon registration in AuthService
                CreatedAt = @event.OccurredAt,
                UpdatedAt = @event.OccurredAt
            };

            await _deliveryAgentRepository.AddAsync(deliveryAgent);

            _logger.LogInformation(
                "Delivery agent registered successfully: AuthUserId={AuthUserId}, Email={Email}, FullName={FullName}",
                @event.UserId, @event.Email, @event.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error registering delivery agent: UserId={UserId}, Email={Email}",
                @event.UserId, @event.Email);
            throw;
        }
    }
}
