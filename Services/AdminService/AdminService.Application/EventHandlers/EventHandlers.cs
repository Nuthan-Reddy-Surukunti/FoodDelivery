using Microsoft.Extensions.Logging;
using FoodDelivery.Shared.Events.Auth;
using FoodDelivery.Shared.Events.Catalog;
using FoodDelivery.Shared.Events.Order;
using MassTransit;

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

// RestaurantApprovedEventHandler
public class RestaurantApprovedEventHandler : IConsumer<RestaurantApprovedEvent>
{
    private readonly ILogger<RestaurantApprovedEventHandler> _logger;
    public RestaurantApprovedEventHandler(ILogger<RestaurantApprovedEventHandler> logger) => _logger = logger;
    public async Task Consume(ConsumeContext<RestaurantApprovedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantApprovedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
        await Task.CompletedTask;
    }
}

// RestaurantRejectedEventHandler
public class RestaurantRejectedEventHandler : IConsumer<RestaurantRejectedEvent>
{
    private readonly ILogger<RestaurantRejectedEventHandler> _logger;
    public RestaurantRejectedEventHandler(ILogger<RestaurantRejectedEventHandler> logger) => _logger = logger;
    public async Task Consume(ConsumeContext<RestaurantRejectedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantRejectedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
        await Task.CompletedTask;
    }
}

// OrderPlacedEventHandler
public class OrderPlacedEventHandler : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;
    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger) => _logger = logger;
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing OrderPlacedEvent: OrderId={OrderId}", @event.OrderId);
        await Task.CompletedTask;
    }
}

// OrderStatusChangedEventHandler
public class OrderStatusChangedEventHandler : IConsumer<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;
    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger) => _logger = logger;
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing OrderStatusChangedEvent: OrderId={OrderId}, NewStatus={NewStatus}", @event.OrderId, @event.NewStatus);
        await Task.CompletedTask;
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
