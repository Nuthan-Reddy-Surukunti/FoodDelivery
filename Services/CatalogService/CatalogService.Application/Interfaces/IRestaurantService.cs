namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Application.DTOs.Restaurant;

public interface IRestaurantService
{
    Task<PaginatedResultDto<RestaurantDto>> GetAllRestaurantsAsync(int pageNumber, int pageSize, string? userRole = null);

    Task<RestaurantDetailDto> GetRestaurantByIdAsync(Guid id, string? userRole = null);

    Task<RestaurantDetailDto> CreateRestaurantAsync(CreateRestaurantDto dto, Guid userId, string userRole);

    Task<RestaurantDetailDto> UpdateRestaurantAsync(Guid id, UpdateRestaurantDto dto, Guid userId, string userRole);

    Task<PaginatedResultDto<RestaurantDto>> GetRestaurantsByCityAsync(string city, int pageNumber, int pageSize, string? userRole = null);

    Task<RestaurantDetailDto> ToggleRestaurantStatusAsync(Guid id);

    Task<List<MenuItemDto>> GetRestaurantMenuAsync(Guid restaurantId);
}
