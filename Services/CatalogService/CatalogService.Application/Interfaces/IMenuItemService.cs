
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.Interfaces;
public interface IMenuItemService
{
    Task<MenuItemDto> GetMenuItemByIdAsync(Guid id, string? userRole = null);

    Task<List<MenuItemDto>> GetMenuItemsByRestaurantAsync(Guid restaurantId, string? userRole = null, Guid? userId = null);

    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto, Guid userId, string userRole);

    Task<MenuItemDto> UpdateMenuItemAsync(UpdateMenuItemDto dto, Guid userId, string userRole);

    Task<bool> DeleteMenuItemAsync(Guid id, Guid userId, string userRole);
}
