namespace CatalogService.Domain.Interfaces;

using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id);

    Task<List<MenuItem>> GetByRestaurantAsync(Guid restaurantId);

    Task<List<MenuItem>> SearchAsync(string query);

    Task<MenuItem> CreateAsync(MenuItem menuItem);

    Task<MenuItem> UpdateAsync(MenuItem menuItem);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}

