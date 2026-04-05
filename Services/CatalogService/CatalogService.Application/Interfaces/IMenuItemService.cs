
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.Interfaces;
public interface IMenuItemService
{
    Task<MenuItemDto> GetMenuItemByIdAsync(Guid id);

    Task<PaginatedResultDto<MenuItemDto>> GetMenuItemsByRestaurantAsync(Guid restaurantId, int pageNumber, int pageSize);

    Task<PaginatedResultDto<MenuItemDto>> GetMenuItemsByCategoryAsync(Guid categoryId, int pageNumber, int pageSize);

    Task<PaginatedResultDto<MenuItemDto>> SearchByNameAsync(string query, Guid restaurantId, int pageNumber, int pageSize);

    Task<PaginatedResultDto<MenuItemDto>> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status, int pageNumber, int pageSize);

    Task<PaginatedResultDto<MenuItemDto>> GetVegItemsAsync(Guid restaurantId, int pageNumber, int pageSize);

    Task<PaginatedResultDto<MenuItemDto>> GetNonVegItemsAsync(Guid restaurantId, int pageNumber, int pageSize);

    Task<PaginatedResultDto<MenuItemDto>> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice, int pageNumber, int pageSize);

    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto, Guid userId, string userRole);

    Task<MenuItemDto> UpdateMenuItemAsync(UpdateMenuItemDto dto, Guid userId, string userRole);

    Task<bool> DeleteMenuItemAsync(Guid id, Guid userId, string userRole);

    Task<MenuItemDto> ToggleAvailabilityAsync(Guid id, ItemAvailabilityStatus status);
}
