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

    public async Task<MenuItemDto> GetMenuItemByIdAsync(Guid id)
    {
        var menuItem = await _repository.GetByIdAsync(id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(id);

        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<PaginatedResultDto<MenuItemDto>> GetMenuItemsByRestaurantAsync(Guid restaurantId, int pageNumber = 1, int pageSize = 10)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(restaurantId);

        var (items, totalCount) = await _repository.GetByRestaurantAsync(restaurantId, pageNumber, pageSize);
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

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto)
    {
        if (dto.Price <= 0)
            throw new InvalidMenuItemPriceException(0m);

        var menuItem = _mapper.Map<MenuItem>(dto);
        var createdItem = await _repository.CreateAsync(menuItem);
        
        return _mapper.Map<MenuItemDto>(createdItem);
    }

    public async Task<MenuItemDto> UpdateMenuItemAsync(UpdateMenuItemDto dto)
    {
        if (dto.Id == Guid.Empty)
            throw new InvalidRestaurantDataException("MenuItem ID is required.");

        if (dto.Price.HasValue && dto.Price <= 0)
            throw new InvalidMenuItemPriceException(0m);

        var menuItem = await _repository.GetByIdAsync(dto.Id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(dto.Id);

        _mapper.Map(dto, menuItem);
        var updatedItem = await _repository.UpdateAsync(menuItem);
        
        return _mapper.Map<MenuItemDto>(updatedItem);
    }

    public async Task<bool> DeleteMenuItemAsync(Guid id)
    {
        var menuItem = await _repository.GetByIdAsync(id);
        if (menuItem == null)
            throw new MenuItemNotFoundException(id);

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
