using QuickBite.Shared.Events.Auth;
using CatalogEvents = QuickBite.Shared.Events.Catalog;
using Microsoft.Extensions.Logging;
using MassTransit;
using CatalogService.Domain.Enums;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.EventHandlers;



/// <summary>
/// Handles UserDeletedEvent from AuthService to delete all restaurants owned by the user
/// </summary>
public class UserDeletedEventHandler : IConsumer<UserDeletedEvent>
{
    private readonly ILogger<UserDeletedEventHandler> _logger;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ICategoryRepository _categoryRepository;

    public UserDeletedEventHandler(
        ILogger<UserDeletedEventHandler> logger,
        IRestaurantRepository restaurantRepository,
        IMenuItemRepository menuItemRepository,
        ICategoryRepository categoryRepository)
    {
        _logger = logger;
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
        _menuItemRepository = menuItemRepository ?? throw new ArgumentNullException(nameof(menuItemRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var @event = context.Message;
        
        // We only care about RestaurantPartners being deleted for cascading restaurant deletion
        if (!@event.Role.Equals("RestaurantPartner", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _logger.LogInformation("Processing UserDeletedEvent for RestaurantPartner: UserId={UserId}, Email={Email}", 
            @event.UserId, @event.Email);

        try
        {
            var restaurants = await _restaurantRepository.GetListByOwnerIdAsync(@event.UserId);
            
            if (restaurants == null || !restaurants.Any())
            {
                _logger.LogInformation("No restaurants found for deleted partner: UserId={UserId}", @event.UserId);
                return;
            }

            _logger.LogInformation("Found {Count} restaurants to delete for user {UserId}", restaurants.Count, @event.UserId);

            foreach (var restaurant in restaurants)
            {
                _logger.LogInformation("Cascading deletion for restaurant: RestaurantId={RestaurantId}, Name={Name}", 
                    restaurant.Id, restaurant.Name);

                // Step 1: Delete all menu items for this restaurant
                var menuItems = await _menuItemRepository.GetByRestaurantAsync(restaurant.Id);
                foreach (var menuItem in menuItems)
                {
                    await _menuItemRepository.DeleteAsync(menuItem.Id);
                }

                // Step 2: Delete all categories for this restaurant
                var categories = await _categoryRepository.GetByRestaurantAsync(restaurant.Id);
                foreach (var category in categories)
                {
                    await _categoryRepository.DeleteAsync(category.Id);
                }

                // Step 3: Delete the restaurant itself
                await _restaurantRepository.DeleteAsync(restaurant.Id);
                
                _logger.LogInformation("Successfully deleted restaurant and its associated data: RestaurantId={RestaurantId}", 
                    restaurant.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserDeletedEvent for UserId={UserId}", @event.UserId);
            throw;
        }
    }
}

/// <summary>
/// Handles RestaurantApprovedEvent from AdminService to sync restaurant status
/// </summary>
public class RestaurantApprovedEventHandler : IConsumer<CatalogEvents.RestaurantApprovedEvent>
{
    private readonly ILogger<RestaurantApprovedEventHandler> _logger;
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantApprovedEventHandler(
        ILogger<RestaurantApprovedEventHandler> logger,
        IRestaurantRepository restaurantRepository)
    {
        _logger = logger;
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
    }

    public async Task Consume(ConsumeContext<CatalogEvents.RestaurantApprovedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantApprovedEvent: RestaurantId={RestaurantId}, Name={Name}", 
            @event.RestaurantId, @event.Name);

        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(@event.RestaurantId);
            if (restaurant == null)
            {
                // Throw exception to trigger retry (handles race condition where approval arrives before creation event)
                throw new InvalidOperationException(
                    $"Restaurant not found: RestaurantId={@event.RestaurantId}. " +
                    $"This may occur if approval event arrives before creation event. Retrying...");
            }

            // Update restaurant status to Active (approved)
            restaurant.Status = RestaurantStatus.Active;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _restaurantRepository.UpdateAsync(restaurant);
            _logger.LogInformation("Restaurant status updated to Active: RestaurantId={RestaurantId}", @event.RestaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RestaurantApprovedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
            throw;
        }
    }
}

/// <summary>
/// Handles RestaurantRejectedEvent from AdminService to sync restaurant status
/// </summary>
public class RestaurantRejectedEventHandler : IConsumer<CatalogEvents.RestaurantRejectedEvent>
{
    private readonly ILogger<RestaurantRejectedEventHandler> _logger;
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantRejectedEventHandler(
        ILogger<RestaurantRejectedEventHandler> logger,
        IRestaurantRepository restaurantRepository)
    {
        _logger = logger;
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
    }

    public async Task Consume(ConsumeContext<CatalogEvents.RestaurantRejectedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantRejectedEvent: RestaurantId={RestaurantId}, Reason={Reason}", 
            @event.RestaurantId, @event.RejectionReason);

        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(@event.RestaurantId);
            if (restaurant == null)
            {
                // Throw exception to trigger retry (handles race condition where rejection arrives before creation event)
                throw new InvalidOperationException(
                    $"Restaurant not found: RestaurantId={@event.RestaurantId}. " +
                    $"This may occur if rejection event arrives before creation event. Retrying...");
            }

            // Update restaurant status to Inactive (rejected)
            restaurant.Status = RestaurantStatus.Inactive;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _restaurantRepository.UpdateAsync(restaurant);
            _logger.LogInformation("Restaurant status updated to Inactive: RestaurantId={RestaurantId}", @event.RestaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RestaurantRejectedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
            throw;
        }
    }
}

/// <summary>
/// Handles RestaurantDeletedEvent from AdminService to delete restaurant and cascade delete all items and categories
/// </summary>
public class RestaurantDeletedEventHandler : IConsumer<CatalogEvents.RestaurantDeletedEvent>
{
    private readonly ILogger<RestaurantDeletedEventHandler> _logger;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ICategoryRepository _categoryRepository;

    public RestaurantDeletedEventHandler(
        ILogger<RestaurantDeletedEventHandler> logger,
        IRestaurantRepository restaurantRepository,
        IMenuItemRepository menuItemRepository,
        ICategoryRepository categoryRepository)
    {
        _logger = logger;
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
        _menuItemRepository = menuItemRepository ?? throw new ArgumentNullException(nameof(menuItemRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public async Task Consume(ConsumeContext<CatalogEvents.RestaurantDeletedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing RestaurantDeletedEvent: RestaurantId={RestaurantId}, Name={Name}", 
            @event.RestaurantId, @event.Name);

        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(@event.RestaurantId);
            if (restaurant == null)
            {
                _logger.LogWarning("Restaurant not found for deletion: RestaurantId={RestaurantId}", @event.RestaurantId);
                return; // Restaurant may have already been deleted or doesn't exist
            }

            // Step 1: Delete all menu items for this restaurant (cascade)
            var menuItems = await _menuItemRepository.GetByRestaurantAsync(@event.RestaurantId);
            foreach (var menuItem in menuItems)
            {
                try
                {
                    await _menuItemRepository.DeleteAsync(menuItem.Id);
                    _logger.LogInformation("Deleted menu item: MenuItemId={MenuItemId}, Name={Name}", 
                        menuItem.Id, menuItem.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting menu item: MenuItemId={MenuItemId}", menuItem.Id);
                    // Continue with other items instead of failing entire deletion
                }
            }

            // Step 2: Delete all categories for this restaurant (cascade)
            var categories = await _categoryRepository.GetByRestaurantAsync(@event.RestaurantId);
            foreach (var category in categories)
            {
                try
                {
                    await _categoryRepository.DeleteAsync(category.Id);
                    _logger.LogInformation("Deleted category: CategoryId={CategoryId}, Name={Name}", 
                        category.Id, category.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting category: CategoryId={CategoryId}", category.Id);
                    // Continue with other categories instead of failing entire deletion
                }
            }

            // Step 3: Delete the restaurant itself
            var deleted = await _restaurantRepository.DeleteAsync(@event.RestaurantId);
            if (deleted)
            {
                _logger.LogInformation("Restaurant hard deleted: RestaurantId={RestaurantId}, Name={Name}", 
                    @event.RestaurantId, @event.Name);
            }
            else
            {
                _logger.LogWarning("Failed to delete restaurant: RestaurantId={RestaurantId}", @event.RestaurantId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RestaurantDeletedEvent: RestaurantId={RestaurantId}", @event.RestaurantId);
            throw;
        }
    }
}
