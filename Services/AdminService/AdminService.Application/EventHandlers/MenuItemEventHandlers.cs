using Microsoft.Extensions.Logging;
using QuickBite.Shared.Events.Catalog;
using MassTransit;
using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.EventHandlers;

public class MenuItemCreatedEventHandler : IConsumer<MenuItemCreatedEvent>
{
    private readonly ILogger<MenuItemCreatedEventHandler> _logger;
    private readonly IMenuItemRepository _menuItemRepository;

    public MenuItemCreatedEventHandler(ILogger<MenuItemCreatedEventHandler> logger, IMenuItemRepository menuItemRepository)
    {
        _logger = logger;
        _menuItemRepository = menuItemRepository ?? throw new ArgumentNullException(nameof(menuItemRepository));
    }

    public async Task Consume(ConsumeContext<MenuItemCreatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing MenuItemCreatedEvent: MenuItemId={MenuItemId}, RestaurantId={RestaurantId}", @event.MenuItemId, @event.RestaurantId);

        try
        {
            var existing = await _menuItemRepository.GetByIdAsync(@event.MenuItemId, context.CancellationToken);
            if (existing != null)
            {
                _logger.LogWarning("MenuItem already exists in AdminService: MenuItemId={MenuItemId}", @event.MenuItemId);
                return;
            }

            var menuItem = new MenuItem
            {
                Id = @event.MenuItemId,
                RestaurantId = @event.RestaurantId,
                Name = @event.Name,
                Description = @event.Description,
                Price = @event.Price,
                IsVeg = @event.IsVeg,
                AvailabilityStatus = @event.AvailabilityStatus,
                CategoryName = @event.CategoryName,
                CreatedAt = DateTime.UtcNow,
                LastSyncedAt = @event.OccurredAt,
                SyncEventId = @event.EventId
            };

            await _menuItemRepository.AddAsync(menuItem, context.CancellationToken);
            _logger.LogInformation("MenuItem synced successfully to AdminService: MenuItemId={MenuItemId}", @event.MenuItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MenuItemCreatedEvent: MenuItemId={MenuItemId}", @event.MenuItemId);
            throw;
        }
    }
}

public class MenuItemUpdatedEventHandler : IConsumer<MenuItemUpdatedEvent>
{
    private readonly ILogger<MenuItemUpdatedEventHandler> _logger;
    private readonly IMenuItemRepository _menuItemRepository;

    public MenuItemUpdatedEventHandler(ILogger<MenuItemUpdatedEventHandler> logger, IMenuItemRepository menuItemRepository)
    {
        _logger = logger;
        _menuItemRepository = menuItemRepository ?? throw new ArgumentNullException(nameof(menuItemRepository));
    }

    public async Task Consume(ConsumeContext<MenuItemUpdatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing MenuItemUpdatedEvent: MenuItemId={MenuItemId}", @event.MenuItemId);

        try
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(@event.MenuItemId, context.CancellationToken);
            if (menuItem == null)
            {
                _logger.LogWarning("MenuItem not found in AdminService for update: MenuItemId={MenuItemId}", @event.MenuItemId);
                return;
            }

            menuItem.Name = @event.Name;
            menuItem.Description = @event.Description;
            menuItem.Price = @event.Price;
            menuItem.IsVeg = @event.IsVeg;
            menuItem.AvailabilityStatus = @event.AvailabilityStatus;
            menuItem.CategoryName = @event.CategoryName;
            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.LastSyncedAt = @event.OccurredAt;
            menuItem.SyncEventId = @event.EventId;

            await _menuItemRepository.UpdateAsync(menuItem, context.CancellationToken);
            _logger.LogInformation("MenuItem updated successfully in AdminService: MenuItemId={MenuItemId}", @event.MenuItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MenuItemUpdatedEvent: MenuItemId={MenuItemId}", @event.MenuItemId);
            throw;
        }
    }
}

public class MenuItemDeletedEventHandler : IConsumer<MenuItemDeletedEvent>
{
    private readonly ILogger<MenuItemDeletedEventHandler> _logger;
    private readonly IMenuItemRepository _menuItemRepository;

    public MenuItemDeletedEventHandler(ILogger<MenuItemDeletedEventHandler> logger, IMenuItemRepository menuItemRepository)
    {
        _logger = logger;
        _menuItemRepository = menuItemRepository ?? throw new ArgumentNullException(nameof(menuItemRepository));
    }

    public async Task Consume(ConsumeContext<MenuItemDeletedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing MenuItemDeletedEvent: MenuItemId={MenuItemId}", @event.MenuItemId);

        try
        {
            var success = await _menuItemRepository.DeleteAsync(@event.MenuItemId, context.CancellationToken);
            if (!success)
            {
                _logger.LogWarning("MenuItem not found in AdminService for deletion: MenuItemId={MenuItemId}", @event.MenuItemId);
            }
            else
            {
                _logger.LogInformation("MenuItem deleted successfully in AdminService: MenuItemId={MenuItemId}", @event.MenuItemId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MenuItemDeletedEvent: MenuItemId={MenuItemId}", @event.MenuItemId);
            throw;
        }
    }
}
