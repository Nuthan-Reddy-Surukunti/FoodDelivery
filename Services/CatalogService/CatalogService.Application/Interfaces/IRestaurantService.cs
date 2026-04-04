namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Application.DTOs.Restaurant;

public interface IRestaurantService
{
    Task<PaginatedResultDto<RestaurantDto>> GetAllRestaurantsAsync(int pageNumber, int pageSize);

    Task<RestaurantDetailDto> GetRestaurantByIdAsync(Guid id);

    Task<RestaurantDetailDto> CreateRestaurantAsync(CreateRestaurantDto dto);

    Task<RestaurantDetailDto> UpdateRestaurantAsync(UpdateRestaurantDto dto);

    Task<bool> DeleteRestaurantAsync(Guid id);

    Task<PaginatedResultDto<RestaurantDto>> GetRestaurantsByCityAsync(string city, int pageNumber, int pageSize);

    Task<RestaurantDetailDto> ToggleRestaurantStatusAsync(Guid id);

    Task<List<MenuItemDto>> GetRestaurantMenuAsync(Guid restaurantId);
}
