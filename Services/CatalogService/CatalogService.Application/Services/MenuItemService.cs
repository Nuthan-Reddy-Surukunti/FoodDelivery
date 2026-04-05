using AutoMapper;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Pagination;
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

    public async Task<PaginatedResultDto<MenuItemDto>> GetMenuItemsByRestaurantAsync(Guid restaurantId, int pageNumber = 1, int pageSize = 10, string? userRole = null)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(restaurantId);

        // Check if restaurant is active unless user is Admin
        if (userRole != "Admin" && restaurant.Status != RestaurantStatus.Active)
            throw new RestaurantNotFoundException(restaurantId);

        var (items, totalCount) = await _repository.GetByRestaurantAsync(restaurantId, pageNumber, pageSize);
        
        // Filter by available status unless user is Admin
        if (userRole != "Admin")
        {
            items = items.Where(i => i.AvailabilityStatus == ItemAvailabilityStatus.Available).ToList();
            totalCount = items.Count;
        }
        
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<MenuItemDto>> GetMenuItemsByCategoryAsync(Guid categoryId, int pageNumber = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetByCategoryAsync(categoryId, pageNumber, pageSize);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<MenuItemDto>> SearchByNameAsync(string query, Guid restaurantId, int pageNumber = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _repository.SearchByNameAsync(query, restaurantId, pageNumber, pageSize);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<MenuItemDto>> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status, int pageNumber = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetByAvailabilityAsync(restaurantId, status, pageNumber, pageSize);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<MenuItemDto>> GetVegItemsAsync(Guid restaurantId, int pageNumber = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetVegItemsAsync(restaurantId, pageNumber, pageSize);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<MenuItemDto>> GetNonVegItemsAsync(Guid restaurantId, int pageNumber = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _repository.GetNonVegItemsAsync(restaurantId, pageNumber, pageSize);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<MenuItemDto>> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice, int pageNumber = 1, int pageSize = 10)
    {
        if (minPrice < 0 || maxPrice < 0)
            throw new InvalidMenuItemPriceException(0m);

        if (minPrice > maxPrice)
            throw new InvalidMenuItemPriceException(0m);

        var (items, totalCount) = await _repository.GetByPriceRangeAsync(restaurantId, minPrice, maxPrice, pageNumber, pageSize);
        var itemDtos = _mapper.Map<List<MenuItemDto>>(items);
        
        return new PaginatedResultDto<MenuItemDto>
        {
            Data = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
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
