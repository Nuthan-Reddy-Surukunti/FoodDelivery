using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using QuickBite.Shared.Events.Saga;

namespace OrderService.Application.Consumers;

/// <summary>
/// Validates both the restaurant and all ordered menu items.
/// Publishes either OrderValidationSucceededEvent or OrderValidationFailedEvent.
/// Consumed as part of the OrderFulfillmentSaga flow.
/// </summary>
public class ValidateOrderConsumer : IConsumer<ValidateOrderCommand>
{
    private readonly IRestaurantValidationService _restaurantValidation;
    private readonly IMenuItemValidationService _menuItemValidation;
    private readonly ILogger<ValidateOrderConsumer> _logger;

    public ValidateOrderConsumer(
        IRestaurantValidationService restaurantValidation,
        IMenuItemValidationService menuItemValidation,
        ILogger<ValidateOrderConsumer> logger)
    {
        _restaurantValidation = restaurantValidation;
        _menuItemValidation = menuItemValidation;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ValidateOrderCommand> context)
    {
        var message = context.Message;
        _logger.LogInformation("Validating order {OrderId} — Restaurant: {RestaurantId}, Items: {ItemCount}",
            message.OrderId, message.RestaurantId, message.Items.Count);

        // ── Step 1: Check the restaurant is Active ──────────────────────────
        var restaurantResult = await _restaurantValidation.ValidateRestaurantAsync(
            message.RestaurantId, context.CancellationToken);

        if (!restaurantResult.IsActive)
        {
            _logger.LogWarning("Order {OrderId} validation FAILED — Restaurant inactive: {Reason}",
                message.OrderId, restaurantResult.ErrorMessage);

            await context.Publish(new OrderValidationFailedEvent
            {
                OrderId = message.OrderId,
                Reason = restaurantResult.ErrorMessage ?? "Restaurant is no longer accepting orders"
            });
            return;
        }

        // ── Step 2: Check every ordered item is Available ───────────────────
        foreach (var item in message.Items)
        {
            var itemResult = await _menuItemValidation.ValidateMenuItemAsync(
                message.RestaurantId, item.MenuItemId, context.CancellationToken);

            if (!itemResult.IsValid || !itemResult.IsAvailable)
            {
                string itemName = string.IsNullOrEmpty(itemResult.ItemName)
                    ? item.MenuItemId.ToString()
                    : itemResult.ItemName;

                _logger.LogWarning("Order {OrderId} validation FAILED — Item '{ItemName}' unavailable",
                    message.OrderId, itemName);

                await context.Publish(new OrderValidationFailedEvent
                {
                    OrderId = message.OrderId,
                    Reason = $"'{itemName}' is currently out of stock or unavailable"
                });
                return;
            }
        }

        // ── All checks passed ────────────────────────────────────────────────
        _logger.LogInformation("Order {OrderId} validation SUCCEEDED", message.OrderId);
        await context.Publish(new OrderValidationSucceededEvent
        {
            OrderId = message.OrderId,
            RestaurantId = message.RestaurantId
        });
    }
}
