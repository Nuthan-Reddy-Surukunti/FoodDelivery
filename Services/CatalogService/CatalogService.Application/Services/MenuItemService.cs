using AutoMapper;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public MenuItemService(IMenuItemRepository repository, IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _repository = repository;
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
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

    public async Task<List<MenuItemDto>> GetMenuItemsByCategoryAsync(Guid categoryId)
    {
        var items = await _repository.GetByCategoryAsync(categoryId);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        return itemDtos;
    }

    public async Task<List<MenuItemDto>> SearchByNameAsync(string query, Guid restaurantId)
    {
        var items = await _repository.SearchByNameAsync(query, restaurantId);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        return itemDtos;
    }

    public async Task<List<MenuItemDto>> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status)
    {
        var items = await _repository.GetByAvailabilityAsync(restaurantId, status);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        return itemDtos;
    }

    public async Task<List<MenuItemDto>> GetVegItemsAsync(Guid restaurantId)
    {
        var items = await _repository.GetVegItemsAsync(restaurantId);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        return itemDtos;
    }

    public async Task<List<MenuItemDto>> GetNonVegItemsAsync(Guid restaurantId)
    {
        var items = await _repository.GetNonVegItemsAsync(restaurantId);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        return itemDtos;
    }

    public async Task<List<MenuItemDto>> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice)
    {
        if (minPrice < 0 || maxPrice < 0)
            throw new InvalidMenuItemPriceException(0m);

        if (minPrice > maxPrice)
            throw new InvalidMenuItemPriceException(0m);

        var items = await _repository.GetByPriceRangeAsync(restaurantId, minPrice, maxPrice);
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

        return await _repository.DeleteAsync(id);
    }

    public async Task<MenuItemDto> ToggleAvailabilityAsync(Guid id, ItemAvailabilityStatus status)
    {
        var menuItem = await _repository.GetByIdAsync(id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(id);

        menuItem.AvailabilityStatus = status;
        var updatedItem = await _repository.UpdateAsync(menuItem);
        
        return _mapper.Map<MenuItemDto>(updatedItem);
    }
}
