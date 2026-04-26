using Microsoft.Extensions.Logging;
using QuickBite.Shared.Events.Auth;
using QuickBite.Shared.Events.Catalog;
using QuickBite.Shared.Events.Order;
using MassTransit;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.EventHandlers;

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
                Currency = "INR",
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

// RestaurantUpdatedEventHandler - Syncs restaurant updates from CatalogService to AdminService
public class RestaurantUpdatedEventHandler : IConsumer<RestaurantUpdatedEvent>
{
    private readonly ILogger<RestaurantUpdatedEventHandler> _logger;
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantUpdatedEventHandler(ILogger<RestaurantUpdatedEventHandler> logger, IRestaurantRepository restaurantRepository)
    {
        _logger = logger;
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
    }

    public async Task Consume(ConsumeContext<RestaurantUpdatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantUpdatedEvent: RestaurantId={RestaurantId}, Name={Name}", 
            @event.RestaurantId, @event.Name);

        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(@event.RestaurantId, context.CancellationToken);
            if (restaurant == null)
            {
                _logger.LogWarning("Restaurant not found in AdminService for update: RestaurantId={RestaurantId}", @event.RestaurantId);
                return;
            }

            restaurant.Name = @event.Name;
            restaurant.Description = @event.Description;
            restaurant.City = @event.City;
            // CuisineType could be mapped if needed, but AdminService Restaurant entity might not have it or just needs basic fields
            
            // Wait, does AdminService.Domain.Entities.Restaurant have City, Description? Yes it does.
            // Let's check what fields we have in Restaurant entity: City, Description, Name.
            // Update those fields:
            
            restaurant.UpdatedAt = DateTime.UtcNow;
            restaurant.LastSyncedAt = @event.OccurredAt;
            restaurant.SyncEventId = @event.EventId;

            await _restaurantRepository.UpdateAsync(restaurant, context.CancellationToken);
            _logger.LogInformation("Restaurant updated successfully in AdminService: RestaurantId={RestaurantId}", @event.RestaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RestaurantUpdatedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
            throw;
        }
    }
}
