using AutoMapper;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using FoodDelivery.Shared.Events.Catalog;
using MassTransit;

namespace CatalogService.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public RestaurantService(IRestaurantRepository repository, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PaginatedResultDto<RestaurantDto>> GetAllRestaurantsAsync(int pageNumber = 1, int pageSize = 10, string? userRole = null)
    {
        var (restaurants, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize);
        
        // Filter by active status unless user is Admin
        if (userRole != "Admin")
        {
            restaurants = restaurants.Where(r => r.Status == Domain.Enums.RestaurantStatus.Active).ToList();
            totalCount = restaurants.Count;
        }
        
        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        
        return new PaginatedResultDto<RestaurantDto>
        {
            Data = restaurantDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RestaurantDetailDto> GetRestaurantByIdAsync(Guid id, string? userRole = null)
    {
        var restaurant = await _repository.GetByIdAsync(id);
        if (restaurant == null)
            throw new RestaurantNotFoundException(id);

        // Check if restaurant is active unless user is Admin
        if (userRole != "Admin" && restaurant.Status != Domain.Enums.RestaurantStatus.Active)
            throw new RestaurantNotFoundException(id);

        return _mapper.Map<RestaurantDetailDto>(restaurant);
    }

    public async Task<RestaurantDetailDto> CreateRestaurantAsync(CreateRestaurantDto dto, Guid userId, string userRole)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidRestaurantDataException("Restaurant name is required.");
        
        // Allow both Admin and RestaurantPartner to create restaurants
        if (userRole != "Admin" && userRole != "RestaurantPartner")
            throw new UnauthorizedAccessException("Only administrators and restaurant partners can create restaurants.");

        var restaurant = _mapper.Map<Restaurant>(dto);

        // Set ownership and status based on role
        if (userRole == "RestaurantPartner")
        {
            // RestaurantPartner creates their own restaurant (requires admin approval)
            restaurant.OwnerId = userId;
            restaurant.Status = Domain.Enums.RestaurantStatus.Pending;
        }
        else if (userRole == "Admin")
        {
            // Admin can create and optionally assign to a RestaurantPartner
            // If OwnerId is specified in DTO, use it; otherwise leave as null/unowned
            if (dto.OwnerId.HasValue)
            {
                restaurant.OwnerId = dto.OwnerId.Value;
            }
            // Admin-created restaurants default to Active (no approval needed)
            restaurant.Status = Domain.Enums.RestaurantStatus.Active;
        }

        var createdRestaurant = await _repository.CreateAsync(restaurant);
        
        // Publish RestaurantCreatedEvent for AdminService to sync (only if restaurant has an owner)
        if (createdRestaurant.OwnerId.HasValue)
        {
            var restaurantCreatedEvent = new RestaurantCreatedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                EventVersion = 1,
                RestaurantId = createdRestaurant.Id,
                OwnerId = createdRestaurant.OwnerId.Value,
                Name = createdRestaurant.Name,
                City = createdRestaurant.City,
                CuisineType = createdRestaurant.CuisineType.ToString()
            };
            
            await _publishEndpoint.Publish(restaurantCreatedEvent);
        }
        
        return _mapper.Map<RestaurantDetailDto>(createdRestaurant);
    }

    public async Task<RestaurantDetailDto> UpdateRestaurantAsync(Guid id, UpdateRestaurantDto dto, Guid userId, string userRole)
    {
        if (id == Guid.Empty)
            throw new InvalidRestaurantDataException("Restaurant ID is required.");

        var restaurant = await _repository.GetByIdAsync(id);
        if (restaurant == null)
            throw new RestaurantNotFoundException(id);
        
        // Authorization checks
        if (userRole == "RestaurantPartner")
        {
            // RestaurantPartner can only update their own restaurant
            if (restaurant.OwnerId != userId)
                throw new UnauthorizedAccessException("You can only update your own restaurant.");
            
            // RestaurantPartner cannot change ownership (OwnerId must be null in their request)
            if (dto.OwnerId.HasValue)
                throw new UnauthorizedAccessException("You cannot change restaurant ownership.");
        }
        else if (userRole != "Admin")
        {
            // Only Admin and RestaurantPartner can update
            throw new UnauthorizedAccessException("You do not have permission to update restaurants.");
        }
        // Admin can update any field including OwnerId (for reassignment)

        _mapper.Map(dto, restaurant);
        var updatedRestaurant = await _repository.UpdateAsync(restaurant);
        
        return _mapper.Map<RestaurantDetailDto>(updatedRestaurant);
    }

    public async Task<PaginatedResultDto<RestaurantDto>> GetRestaurantsByCityAsync(string city, int pageNumber = 1, int pageSize = 10, string? userRole = null)
    {
        var (restaurants, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize);
        var filteredRestaurants = restaurants.Where(r => r.City == city).ToList();
        
        // Filter by active status unless user is Admin
        if (userRole != "Admin")
        {
            filteredRestaurants = filteredRestaurants.Where(r => r.Status == Domain.Enums.RestaurantStatus.Active).ToList();
        }
        
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
