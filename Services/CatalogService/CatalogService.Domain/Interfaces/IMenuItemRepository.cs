namespace CatalogService.Domain.Interfaces;

using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id);

    Task<(List<MenuItem>, int)> GetByRestaurantAsync(Guid restaurantId, int pageNumber, int pageSize);

    Task<(List<MenuItem>, int)> GetByCategoryAsync(Guid categoryId, int pageNumber, int pageSize);

    Task<(List<MenuItem>, int)> SearchByNameAsync(string query, Guid restaurantId, int pageNumber, int pageSize);

    Task<(List<MenuItem>, int)> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status, int pageNumber, int pageSize);

    Task<(List<MenuItem>, int)> GetVegItemsAsync(Guid restaurantId, int pageNumber, int pageSize);

    Task<(List<MenuItem>, int)> GetNonVegItemsAsync(Guid restaurantId, int pageNumber, int pageSize);

    Task<(List<MenuItem>, int)> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice, int pageNumber, int pageSize);

    Task<MenuItem> CreateAsync(MenuItem menuItem);

    Task<MenuItem> UpdateAsync(MenuItem menuItem);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}
