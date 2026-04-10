using Microsoft.Extensions.Logging;
using FoodDelivery.Shared.Events.Auth;
using FoodDelivery.Shared.Events.Catalog;
using FoodDelivery.Shared.Events.Order;
using MassTransit;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.EventHandlers;

// UserRegisteredEventHandler
public class UserRegisteredEventHandler : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;
    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger) => _logger = logger;
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing UserRegisteredEvent: UserId={UserId}", @event.UserId);
        await Task.CompletedTask;
    }
}

// RestaurantCreatedEventHandler - Syncs restaurants created by partners into AdminService
public class RestaurantCreatedEventHandler : IConsumer<RestaurantCreatedEvent>
{
    private readonly ILogger<RestaurantCreatedEventHandler> _logger;
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantCreatedEventHandler(ILogger<RestaurantCreatedEventHandler> logger, IRestaurantRepository restaurantRepository)
    {
        _logger = logger;
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
    }

    public async Task Consume(ConsumeContext<RestaurantCreatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantCreatedEvent: RestaurantId={RestaurantId}, Name={Name}, OwnerId={OwnerId}", 
            @event.RestaurantId, @event.Name, @event.OwnerId);

        try
        {
            // Check if restaurant already exists (idempotent)
            var existingRestaurant = await _restaurantRepository.GetByIdAsync(@event.RestaurantId, context.CancellationToken);
            if (existingRestaurant != null)
            {
                _logger.LogWarning("Restaurant already exists in AdminService: RestaurantId={RestaurantId}", @event.RestaurantId);
                return;
            }

            // Create restaurant record in AdminService with Pending status (awaiting approval)
            var restaurant = new Restaurant
            {
                Id = @event.RestaurantId,
                OwnerId = @event.OwnerId,
                Name = @event.Name,
                City = @event.City,
                Description = string.Empty,
                Street = string.Empty,
                State = string.Empty,
                ZipCode = string.Empty,
                Country = string.Empty,
                Email = string.Empty,
                Phone = string.Empty,
                Status = RestaurantStatus.Pending, // Requires admin approval
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastSyncedAt = @event.OccurredAt,
                SyncEventId = @event.EventId
            };

            await _restaurantRepository.AddAsync(restaurant, context.CancellationToken);
            _logger.LogInformation("Restaurant synced successfully to AdminService: RestaurantId={RestaurantId}", @event.RestaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RestaurantCreatedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
            throw;
        }
    }
}

// RestaurantApprovedEventHandler - Logs approval events (approval happens in AdminService endpoint, just audit here)
public class RestaurantApprovedEventHandler : IConsumer<RestaurantApprovedEvent>
{
    private readonly ILogger<RestaurantApprovedEventHandler> _logger;
    
    public RestaurantApprovedEventHandler(ILogger<RestaurantApprovedEventHandler> logger) => _logger = logger;
    
    public async Task Consume(ConsumeContext<RestaurantApprovedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Restaurant approved successfully: RestaurantId={RestaurantId}, Name={Name}, ApprovedBy={ApprovedBy}", 
            @event.RestaurantId, @event.Name, @event.ApprovedBy);
        await Task.CompletedTask;
    }
}

// RestaurantRejectedEventHandler - Logs rejection events (rejection happens in AdminService endpoint, just audit here)
public class RestaurantRejectedEventHandler : IConsumer<RestaurantRejectedEvent>
{
    private readonly ILogger<RestaurantRejectedEventHandler> _logger;
    
    public RestaurantRejectedEventHandler(ILogger<RestaurantRejectedEventHandler> logger) => _logger = logger;
    
    public async Task Consume(ConsumeContext<RestaurantRejectedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Restaurant rejected: RestaurantId={RestaurantId}, Name={Name}, Reason={Reason}", 
            @event.RestaurantId, @event.Name, @event.RejectionReason);
        await Task.CompletedTask;
    }
}

// OrderPlacedEventHandler
public class OrderPlacedEventHandler : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;
    private readonly IOrderRepository _orderRepository;

    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger, IOrderRepository orderRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing OrderPlacedEvent: OrderId={OrderId}, UserId={UserId}", @event.OrderId, @event.UserId);

        try
        {
            var order = new Order
            {
                Id = @event.OrderId,
                CustomerId = @event.UserId,
                RestaurantId = @event.RestaurantId,
                Status = OrderStatus.Pending,
                TotalAmount = @event.TotalAmount,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastSyncedAt = @event.OccurredAt,
                SyncEventId = @event.EventId
            };

            await _orderRepository.AddAsync(order, context.CancellationToken);
            _logger.LogInformation("Order persisted successfully: OrderId={OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderPlacedEvent: OrderId={OrderId}", @event.OrderId);
            throw;
        }
    }
}

// OrderStatusChangedEventHandler
public class OrderStatusChangedEventHandler : IConsumer<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;
    private readonly IOrderRepository _orderRepository;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger, IOrderRepository orderRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing OrderStatusChangedEvent: OrderId={OrderId}, NewStatus={NewStatus}", @event.OrderId, @event.NewStatus);

        try
        {
            var order = await _orderRepository.GetByIdAsync(@event.OrderId, context.CancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order not found: OrderId={OrderId}", @event.OrderId);
                return;
            }

            // Map event status string to OrderStatus enum
            if (Enum.TryParse<OrderStatus>(@event.NewStatus, ignoreCase: true, out var newStatus))
            {
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                order.LastSyncedAt = @event.OccurredAt;
                order.SyncEventId = @event.EventId;

                await _orderRepository.UpdateAsync(order, context.CancellationToken);
                _logger.LogInformation("Order status updated successfully: OrderId={OrderId}, Status={Status}", @event.OrderId, @event.NewStatus);
            }
            else
            {
                _logger.LogWarning("Invalid status value in event: OrderId={OrderId}, Status={Status}", @event.OrderId, @event.NewStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderStatusChangedEvent: OrderId={OrderId}", @event.OrderId);
            throw;
        }
    }
}

// OrderCancelledEventHandler
public class OrderCancelledEventHandler : IConsumer<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;
    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger) => _logger = logger;
    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing OrderCancelledEvent: OrderId={OrderId}", @event.OrderId);
        await Task.CompletedTask;
    }
}
