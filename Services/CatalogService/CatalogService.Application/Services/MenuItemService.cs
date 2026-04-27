using AutoMapper;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Domain.Interfaces;
using MassTransit;
using QuickBite.Shared.Events.Catalog;

namespace CatalogService.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public MenuItemService(
        IMenuItemRepository repository, 
        IRestaurantRepository restaurantRepository, 
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<MenuItemDto> GetMenuItemByIdAsync(Guid id, string? userRole = null)
    {
        var menuItem = await _repository.GetByIdAsync(id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(id);

        // Check if parent restaurant is active and item is available unless user is Admin
        if (userRole != "Admin")
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(menuItem.RestaurantId);
            if (restaurant == null || restaurant.Status != RestaurantStatus.Active)
                throw new MenuItemNotFoundException(id);
            
            if (menuItem.AvailabilityStatus != ItemAvailabilityStatus.Available)
                throw new MenuItemNotFoundException(id);
        }

        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<List<MenuItemDto>> GetMenuItemsByRestaurantAsync(Guid restaurantId, string? userRole = null, Guid? userId = null)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(restaurantId);

        // Allow RestaurantPartner to see items for their own restaurant (any status)
        // Allow Admin to see all items. Other users only see items for Active restaurants
        if (userRole == "RestaurantPartner" && userId.HasValue && restaurant.OwnerId == userId)
        {
            // RestaurantPartner can access their own restaurant even if Pending
        }
        else if (userRole != "Admin" && restaurant.Status != RestaurantStatus.Active)
        {
            // Non-owners and non-admins cannot access non-active restaurants
            throw new RestaurantNotFoundException(restaurantId);
        }

        var items = await _repository.GetByRestaurantAsync(restaurantId);
        
        // Filter by available status unless user is Admin or restaurant owner
        if (userRole != "Admin" && !(userRole == "RestaurantPartner" && userId.HasValue && restaurant.OwnerId == userId))
        {
            items = items.Where(i => i.AvailabilityStatus == ItemAvailabilityStatus.Available).ToList();
        }
        
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        return itemDtos;
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto, Guid userId, string userRole)
    {
        if (dto.Price <= 0)
            throw new InvalidMenuItemPriceException(0m);
        
        // Get parent restaurant to validate ownership
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(dto.RestaurantId);
        
        // RestaurantPartner can only create items for their own restaurant
        if (userRole == "RestaurantPartner" && restaurant.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only create items for your own restaurant.");

        var menuItem = _mapper.Map<MenuItem>(dto);
        var createdItem = await _repository.CreateAsync(menuItem);
        
        var menuItemCreatedEvent = new MenuItemCreatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            MenuItemId = createdItem.Id,
            RestaurantId = createdItem.RestaurantId,
            Name = createdItem.Name,
            Description = createdItem.Description ?? string.Empty,
            Price = createdItem.Price,
            IsVeg = createdItem.IsVeg,
            AvailabilityStatus = createdItem.AvailabilityStatus.ToString(),
            CategoryName = createdItem.Category?.Name ?? string.Empty
        };
        await _publishEndpoint.Publish(menuItemCreatedEvent);
        
        return _mapper.Map<MenuItemDto>(createdItem);
    }

    public async Task<MenuItemDto> UpdateMenuItemAsync(UpdateMenuItemDto dto, Guid userId, string userRole)
    {
        if (dto.Id == Guid.Empty)
            throw new InvalidRestaurantDataException("MenuItem ID is required.");

        if (dto.Price.HasValue && dto.Price <= 0)
            throw new InvalidMenuItemPriceException(0m);

        var menuItem = await _repository.GetByIdAsync(dto.Id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(dto.Id);
        
        // Get parent restaurant to validate ownership
        var restaurant = await _restaurantRepository.GetByIdAsync(menuItem.RestaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(menuItem.RestaurantId);
        
        // RestaurantPartner can only update items in their own restaurant
        if (userRole == "RestaurantPartner" && restaurant.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only update items in your own restaurant.");

        _mapper.Map(dto, menuItem);
        var updatedItem = await _repository.UpdateAsync(menuItem);
        
        var menuItemUpdatedEvent = new MenuItemUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            MenuItemId = updatedItem.Id,
            RestaurantId = updatedItem.RestaurantId,
            Name = updatedItem.Name,
            Description = updatedItem.Description ?? string.Empty,
            Price = updatedItem.Price,
            IsVeg = updatedItem.IsVeg,
            AvailabilityStatus = updatedItem.AvailabilityStatus.ToString(),
            CategoryName = updatedItem.Category?.Name ?? string.Empty
        };
        await _publishEndpoint.Publish(menuItemUpdatedEvent);
        
        return _mapper.Map<MenuItemDto>(updatedItem);
    }

    public async Task<bool> DeleteMenuItemAsync(Guid id, Guid userId, string userRole)
    {
        var menuItem = await _repository.GetByIdAsync(id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(id);
        
        // Get parent restaurant to validate ownership
        var restaurant = await _restaurantRepository.GetByIdAsync(menuItem.RestaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(menuItem.RestaurantId);
        
        // RestaurantPartner can only delete items from their own restaurant
        if (userRole == "RestaurantPartner" && restaurant.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only delete items from your own restaurant.");

        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            var menuItemDeletedEvent = new MenuItemDeletedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                EventVersion = 1,
                MenuItemId = id,
                RestaurantId = menuItem.RestaurantId
            };
            await _publishEndpoint.Publish(menuItemDeletedEvent);
        }
        
        return deleted;
    }
}
