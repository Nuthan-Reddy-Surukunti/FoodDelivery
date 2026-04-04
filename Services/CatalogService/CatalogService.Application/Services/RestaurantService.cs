using AutoMapper;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _repository;
    private readonly IMapper _mapper;

    public RestaurantService(IRestaurantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<RestaurantDto>> GetAllRestaurantsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var (restaurants, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize);
        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        
        return new PaginatedResultDto<RestaurantDto>
        {
            Data = restaurantDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RestaurantDetailDto> GetRestaurantByIdAsync(Guid id)
    {
        var restaurant = await _repository.GetByIdAsync(id);
        if (restaurant == null)
            throw new RestaurantNotFoundException(id);

        return _mapper.Map<RestaurantDetailDto>(restaurant);
    }

    public async Task<RestaurantDetailDto> CreateRestaurantAsync(CreateRestaurantDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidRestaurantDataException("Restaurant name is required.");

        var restaurant = _mapper.Map<Restaurant>(dto);
        var createdRestaurant = await _repository.CreateAsync(restaurant);
        
        return _mapper.Map<RestaurantDetailDto>(createdRestaurant);
    }

    public async Task<RestaurantDetailDto> UpdateRestaurantAsync(UpdateRestaurantDto dto)
    {
        if (dto.Id == Guid.Empty)
            throw new InvalidRestaurantDataException("Restaurant ID is required.");

        var restaurant = await _repository.GetByIdAsync(dto.Id);
        if (restaurant == null)
            throw new RestaurantNotFoundException(dto.Id);

        _mapper.Map(dto, restaurant);
        var updatedRestaurant = await _repository.UpdateAsync(restaurant);
        
        return _mapper.Map<RestaurantDetailDto>(updatedRestaurant);
    }

    public async Task<bool> DeleteRestaurantAsync(Guid id)
    {
        var restaurant = await _repository.GetByIdAsync(id);
        if (restaurant == null)
            throw new RestaurantNotFoundException(id);

        return await _repository.DeleteAsync(id);
    }

    public async Task<PaginatedResultDto<RestaurantDto>> GetRestaurantsByCityAsync(string city, int pageNumber = 1, int pageSize = 10)
    {
        var (restaurants, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize);
        var filteredRestaurants = restaurants.Where(r => r.City == city).ToList();
        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(filteredRestaurants);
        
        return new PaginatedResultDto<RestaurantDto>
        {
            Data = restaurantDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = filteredRestaurants.Count
        };
    }

    public async Task<RestaurantDetailDto> ToggleRestaurantStatusAsync(Guid id)
    {
        var restaurant = await _repository.GetByIdAsync(id);
        if (restaurant == null)
            throw new RestaurantNotFoundException(id);

        restaurant.Status = restaurant.Status == Domain.Enums.RestaurantStatus.Active 
            ? Domain.Enums.RestaurantStatus.Inactive 
            : Domain.Enums.RestaurantStatus.Active;

        var updatedRestaurant = await _repository.UpdateAsync(restaurant);
        return _mapper.Map<RestaurantDetailDto>(updatedRestaurant);
    }

    public async Task<List<DTOs.MenuItem.MenuItemDto>> GetRestaurantMenuAsync(Guid restaurantId)
    {
        var restaurant = await _repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(restaurantId);

        return _mapper.Map<List<DTOs.MenuItem.MenuItemDto>>(restaurant.MenuItems);
    }
}
