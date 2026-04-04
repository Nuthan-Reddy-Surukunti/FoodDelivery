namespace CatalogService.Domain.Interfaces;

using CatalogService.Domain.Entities;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);

    Task<List<Category>> GetByRestaurantAsync(Guid restaurantId);

    Task<Category?> GetByNameAsync(string name, Guid restaurantId);

    Task<Category> CreateAsync(Category category);

    Task<Category> UpdateAsync(Category category);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);

    Task<bool> ExistsByNameAsync(string name, Guid restaurantId);
}
