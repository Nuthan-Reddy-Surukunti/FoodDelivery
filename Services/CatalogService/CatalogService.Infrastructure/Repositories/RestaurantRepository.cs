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

    public async Task<(List<Restaurant>, int)> GetAllAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Restaurants.CountAsync();
        var restaurants = await _context.Restaurants
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (restaurants, totalCount);
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

    public async Task<(List<Restaurant>, int)> SearchByNameAsync(string query, int pageNumber, int pageSize)
    {
        var lowerQuery = query.ToLower();
        
        var totalCount = await _context.Restaurants
            .Where(r => r.Name.ToLower().Contains(lowerQuery))
            .CountAsync();

        var restaurants = await _context.Restaurants
            .Where(r => r.Name.ToLower().Contains(lowerQuery))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (restaurants, totalCount);
    }

    public async Task<(List<Restaurant>, int)> GetByCuisineAsync(CuisineType cuisine, int pageNumber, int pageSize)
    {
        var totalCount = await _context.Restaurants
            .Where(r => r.CuisineType == cuisine)
            .CountAsync();

        var restaurants = await _context.Restaurants
            .Where(r => r.CuisineType == cuisine)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (restaurants, totalCount);
    }

    public async Task<(List<Restaurant>, int)> GetByStatusAsync(RestaurantStatus status, int pageNumber, int pageSize)
    {
        var totalCount = await _context.Restaurants
            .Where(r => r.Status == status)
            .CountAsync();

        var restaurants = await _context.Restaurants
            .Where(r => r.Status == status)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (restaurants, totalCount);
    }

    public async Task<(List<Restaurant>, int)> GetByRatingAsync(decimal minRating, int pageNumber, int pageSize)
    {
        var totalCount = await _context.Restaurants
            .Where(r => r.Rating >= minRating)
            .CountAsync();

        var restaurants = await _context.Restaurants
            .Where(r => r.Rating >= minRating)
            .OrderByDescending(r => r.Rating)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (restaurants, totalCount);
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
