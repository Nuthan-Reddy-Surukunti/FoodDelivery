namespace CatalogService.Domain.Interfaces;

using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;

public interface IRestaurantRepository
{
    Task<(List<Restaurant>, int)> GetAllAsync(int pageNumber, int pageSize);

    Task<Restaurant?> GetByIdAsync(Guid id);

    Task<Restaurant?> GetByNameAsync(string name);

    Task<(List<Restaurant>, int)> SearchByNameAsync(string query, int pageNumber, int pageSize);

    Task<(List<Restaurant>, int)> GetByCuisineAsync(CuisineType cuisine, int pageNumber, int pageSize);

    Task<(List<Restaurant>, int)> GetNearbyAsync(double latitude, double longitude, double radiusInKm, int pageNumber, int pageSize);

    Task<(List<Restaurant>, int)> GetByStatusAsync(RestaurantStatus status, int pageNumber, int pageSize);

    Task<(List<Restaurant>, int)> GetByRatingAsync(decimal minRating, int pageNumber, int pageSize);

    Task<Restaurant> CreateAsync(Restaurant restaurant);

    Task<Restaurant> UpdateAsync(Restaurant restaurant);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}
