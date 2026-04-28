namespace CatalogService.Domain.Interfaces;

using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;

public interface IRestaurantRepository
{
    Task<List<Restaurant>> GetAllAsync();
    
    Task<List<Restaurant>> GetFilteredAsync(RestaurantStatus? status, string? searchTerm, string? cuisine, bool? isVegetarianOnly);

    Task<Restaurant?> GetByIdAsync(Guid id);

    Task<Restaurant?> GetByNameAsync(string name);

    Task<Restaurant?> GetByOwnerIdAsync(Guid ownerId);
    
    Task<List<Restaurant>> GetListByOwnerIdAsync(Guid ownerId);

    Task<List<Restaurant>> SearchByNameAsync(string query);

    Task<List<Restaurant>> GetByCuisineAsync(CuisineType cuisine);

    Task<List<Restaurant>> GetByStatusAsync(RestaurantStatus status);

    Task<List<Restaurant>> GetByRatingAsync(decimal minRating);

    Task<Restaurant> CreateAsync(Restaurant restaurant);

    Task<Restaurant> UpdateAsync(Restaurant restaurant);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}
