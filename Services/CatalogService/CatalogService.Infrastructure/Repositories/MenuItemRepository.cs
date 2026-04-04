using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly CatalogDbContext _context;

    public MenuItemRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(List<MenuItem>, int)> GetByRestaurantAsync(Guid restaurantId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .Include(m => m.Category)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<MenuItem>, int)> GetByCategoryAsync(Guid categoryId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.CategoryId == categoryId)
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.CategoryId == categoryId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<MenuItem>, int)> SearchByNameAsync(string query, Guid restaurantId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && EF.Functions.Like(m.Name, $"%{query}%"))
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && EF.Functions.Like(m.Name, $"%{query}%"))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<MenuItem>, int)> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.AvailabilityStatus == status)
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.AvailabilityStatus == status)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<MenuItem>, int)> GetVegItemsAsync(Guid restaurantId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsVeg == true)
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsVeg == true)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<MenuItem>, int)> GetNonVegItemsAsync(Guid restaurantId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsVeg == false)
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsVeg == false)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<MenuItem>, int)> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice, int pageNumber, int pageSize)
    {
        var totalCount = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.Price >= minPrice && m.Price <= maxPrice)
            .CountAsync();

        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.Price >= minPrice && m.Price <= maxPrice)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<MenuItem> CreateAsync(MenuItem menuItem)
    {
        menuItem.CreatedAt = DateTime.UtcNow;
        menuItem.UpdatedAt = DateTime.UtcNow;

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        return menuItem;
    }

    public async Task<MenuItem> UpdateAsync(MenuItem menuItem)
    {
        menuItem.UpdatedAt = DateTime.UtcNow;

        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync();

        return menuItem;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem == null)
            return false;

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.MenuItems.AnyAsync(m => m.Id == id);
    }
}
