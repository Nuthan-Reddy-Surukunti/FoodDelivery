namespace CatalogService.Domain.Interfaces;

using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id);

    Task<List<MenuItem>> GetByRestaurantAsync(Guid restaurantId);

    Task<List<MenuItem>> GetByCategoryAsync(Guid categoryId);

    Task<List<MenuItem>> SearchByNameAsync(string query, Guid restaurantId);

    Task<List<MenuItem>> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status);

    Task<List<MenuItem>> GetVegItemsAsync(Guid restaurantId);

    Task<List<MenuItem>> GetNonVegItemsAsync(Guid restaurantId);

    Task<List<MenuItem>> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice);

    Task<MenuItem> CreateAsync(MenuItem menuItem);

    Task<MenuItem> UpdateAsync(MenuItem menuItem);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}
