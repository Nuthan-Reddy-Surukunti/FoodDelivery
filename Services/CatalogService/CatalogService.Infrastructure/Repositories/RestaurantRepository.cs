using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly CatalogDbContext _context;

    public RestaurantRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<Restaurant>> GetAllAsync()
    {
        var restaurants = await _context.Restaurants
            .ToListAsync();

        return restaurants;
    }

    public async Task<List<Restaurant>> GetFilteredAsync(RestaurantStatus? status, string? searchTerm, string? cuisine, bool? isVegetarianOnly)
    {
        var query = _context.Restaurants.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerTerm = searchTerm.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(lowerTerm) || r.Description.ToLower().Contains(lowerTerm));
        }

        if (!string.IsNullOrWhiteSpace(cuisine))
        {
            if (Enum.TryParse<CuisineType>(cuisine, true, out var cuisineType))
            {
                query = query.Where(r => r.CuisineType == cuisineType);
            }
        }

        if (isVegetarianOnly.HasValue && isVegetarianOnly.Value)
        {
            query = query.Where(r => r.MenuItems.All(m => m.IsVegetarian));
        }

        return await query.ToListAsync();
    }

    public async Task<Restaurant?> GetByIdAsync(Guid id)
    {
        return await _context.Restaurants
            .Include(r => r.MenuItems)
            .Include(r => r.Categories)
            .Include(r => r.OperatingHours)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Restaurant?> GetByNameAsync(string name)
    {
        return await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Restaurant?> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Restaurants
            .Include(r => r.MenuItems)
            .Include(r => r.Categories)
            .Include(r => r.OperatingHours)
            .Where(r => r.OwnerId == ownerId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Restaurant>> GetListByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Restaurants
            .Where(r => r.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<Restaurant>> SearchByNameAsync(string query)
    {
        var lowerQuery = query.ToLower();

        var restaurants = await _context.Restaurants
            .Where(r => r.Name.ToLower().Contains(lowerQuery))
            .ToListAsync();

        return restaurants;
    }

    public async Task<List<Restaurant>> GetByCuisineAsync(CuisineType cuisine)
    {
        var restaurants = await _context.Restaurants
            .Where(r => r.CuisineType == cuisine)
            .ToListAsync();

        return restaurants;
    }

    public async Task<List<Restaurant>> GetByStatusAsync(RestaurantStatus status)
    {
        var restaurants = await _context.Restaurants
            .Where(r => r.Status == status)
            .ToListAsync();

        return restaurants;
    }

    public async Task<List<Restaurant>> GetByRatingAsync(decimal minRating)
    {
        var restaurants = await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Active && r.Rating >= minRating)
            .OrderByDescending(r => r.Rating)
            .ToListAsync();

        return restaurants;
    }

    public async Task<Restaurant> CreateAsync(Restaurant restaurant)
    {
        restaurant.CreatedAt = DateTime.UtcNow;
        restaurant.UpdatedAt = DateTime.UtcNow;

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        return restaurant;
    }

    public async Task<Restaurant> UpdateAsync(Restaurant restaurant)
    {
        restaurant.UpdatedAt = DateTime.UtcNow;

        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync();

        return restaurant;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
            return false;

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == id);
    }
}
