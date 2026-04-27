using AutoMapper;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using QuickBite.Shared.Events.Catalog;
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

    public async Task<List<RestaurantDto>> GetAllRestaurantsAsync(string? userRole = null)
    {
        var restaurants = await _repository.GetAllAsync();
        
        // Filter by active status unless user is Admin
        if (userRole != "Admin")
        {
            restaurants = restaurants.Where(r => r.Status == Domain.Enums.RestaurantStatus.Active).ToList();
        }
        
        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        return restaurantDtos;
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

    public async Task<RestaurantDetailDto> GetRestaurantByOwnerAsync(Guid ownerId, string userRole)
    {
        if (userRole != "RestaurantPartner" && userRole != "Admin")
            throw new UnauthorizedAccessException("Only administrators and restaurant partners can access owner restaurants.");

        var restaurant = await _repository.GetByOwnerIdAsync(ownerId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(ownerId);

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
        
        // Store the current status before any changes
        var originalStatus = restaurant.Status;
        
        // Authorization checks
        if (userRole == "RestaurantPartner")
        {
            // RestaurantPartner can only update their own restaurant
            if (restaurant.OwnerId != userId)
                throw new UnauthorizedAccessException("You can only update your own restaurant.");
            
            // RestaurantPartner cannot change ownership (OwnerId must be null in their request)
            if (dto.OwnerId.HasValue)
                throw new UnauthorizedAccessException("You cannot change restaurant ownership.");
            
            // RestaurantPartner cannot change status - once approved, they can edit without re-approval
            if (dto.Status.HasValue)
                throw new UnauthorizedAccessException("You cannot change restaurant status. Once approved by admin, you can update any other details freely.");
            
            // Map DTO to restaurant (Status is ignored in mapping)
            _mapper.Map(dto, restaurant);
            
            // Double-check: restore original status for extra safety
            restaurant.Status = originalStatus;
        }
        else if (userRole == "Admin")
        {
            // Admin can update any field including status and ownership
            _mapper.Map(dto, restaurant);
            
            // If admin explicitly provided a new status, apply it
            if (dto.Status.HasValue)
            {
                restaurant.Status = dto.Status.Value;
            }
            else
            {
                // If no status provided, preserve current status
                restaurant.Status = originalStatus;
            }
        }
        else
        {
            // Only Admin and RestaurantPartner can update
            throw new UnauthorizedAccessException("You do not have permission to update restaurants.");
        }
        
        var updatedRestaurant = await _repository.UpdateAsync(restaurant);
        
        var restaurantUpdatedEvent = new RestaurantUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            RestaurantId = updatedRestaurant.Id,
            Name = updatedRestaurant.Name,
            Description = updatedRestaurant.Description ?? string.Empty,
            City = updatedRestaurant.City ?? string.Empty,
            CuisineType = updatedRestaurant.CuisineType.ToString(),
            Address = updatedRestaurant.Address ?? string.Empty,
            ContactPhone = updatedRestaurant.ContactPhone ?? string.Empty,
            ContactEmail = updatedRestaurant.ContactEmail ?? string.Empty
        };
        await _publishEndpoint.Publish(restaurantUpdatedEvent);
        
        return _mapper.Map<RestaurantDetailDto>(updatedRestaurant);
    }
}
