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

    public async Task<List<MenuItem>> GetByRestaurantAsync(Guid restaurantId)
    {
        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .Include(m => m.Category)
            .ToListAsync();

        return items;
    }

    public async Task<List<MenuItem>> GetByCategoryAsync(Guid categoryId)
    {
        var items = await _context.MenuItems
            .Where(m => m.CategoryId == categoryId)
            .ToListAsync();

        return items;
    }

    public async Task<List<MenuItem>> SearchByNameAsync(string query, Guid restaurantId)
    {
        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && EF.Functions.Like(m.Name, $"%{query}%"))
            .ToListAsync();

        return items;
    }

    public async Task<List<MenuItem>> GetByAvailabilityAsync(Guid restaurantId, ItemAvailabilityStatus status)
    {
        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.AvailabilityStatus == status)
            .ToListAsync();

        return items;
    }

    public async Task<List<MenuItem>> GetVegItemsAsync(Guid restaurantId)
    {
        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsVeg == true)
            .ToListAsync();

        return items;
    }

    public async Task<List<MenuItem>> GetNonVegItemsAsync(Guid restaurantId)
    {
        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsVeg == false)
            .ToListAsync();

        return items;
    }

    public async Task<List<MenuItem>> GetByPriceRangeAsync(Guid restaurantId, decimal minPrice, decimal maxPrice)
    {
        var items = await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.Price >= minPrice && m.Price <= maxPrice)
            .ToListAsync();

        return items;
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
