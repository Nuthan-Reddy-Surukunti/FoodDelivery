
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.Interfaces;
public interface IMenuItemService
{
    Task<MenuItemDto> GetMenuItemByIdAsync(Guid id, string? userRole = null);

    Task<List<MenuItemDto>> GetMenuItemsByRestaurantAsync(Guid restaurantId, string? userRole = null, Guid? userId = null);

    Task<List<MenuItemDto>> GetMenuItemsByCategoryAsync(Guid categoryId);

    Task<List<MenuItemDto>> SearchByNameAsync(string query, Guid restaurantId);

    Task<List<MenuItemDto>> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status);

    Task<List<MenuItemDto>> GetVegItemsAsync(Guid restaurantId);

    Task<List<MenuItemDto>> GetNonVegItemsAsync(Guid restaurantId);

    Task<List<MenuItemDto>> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice);

    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto, Guid userId, string userRole);

    Task<MenuItemDto> UpdateMenuItemAsync(UpdateMenuItemDto dto, Guid userId, string userRole);

    Task<bool> DeleteMenuItemAsync(Guid id, Guid userId, string userRole);

    Task<MenuItemDto> ToggleAvailabilityAsync(Guid id, ItemAvailabilityStatus status);
}
