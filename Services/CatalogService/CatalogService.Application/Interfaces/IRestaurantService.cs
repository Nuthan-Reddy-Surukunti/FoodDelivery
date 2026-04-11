namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Restaurant;

public interface IRestaurantService
{
    Task<List<RestaurantDto>> GetAllRestaurantsAsync(string? userRole = null);

    Task<RestaurantDetailDto> GetRestaurantByIdAsync(Guid id, string? userRole = null);

    Task<RestaurantDetailDto> CreateRestaurantAsync(CreateRestaurantDto dto, Guid userId, string userRole);

    Task<RestaurantDetailDto> UpdateRestaurantAsync(Guid id, UpdateRestaurantDto dto, Guid userId, string userRole);

    Task<List<RestaurantDto>> GetRestaurantsByCityAsync(string city, string? userRole = null);

    Task<RestaurantDetailDto> ToggleRestaurantStatusAsync(Guid id);

    Task<List<MenuItemDto>> GetRestaurantMenuAsync(Guid restaurantId);
}
