using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Category>> GetByRestaurantAsync(Guid restaurantId)
    {
        return await _context.Categories
            .Where(c => c.RestaurantId == restaurantId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Category?> GetByNameAsync(string name, Guid restaurantId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.RestaurantId == restaurantId && c.Name == name);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid restaurantId)
    {
        return await _context.Categories
            .AnyAsync(c => c.RestaurantId == restaurantId && c.Name == name);
    }
}
