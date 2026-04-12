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
