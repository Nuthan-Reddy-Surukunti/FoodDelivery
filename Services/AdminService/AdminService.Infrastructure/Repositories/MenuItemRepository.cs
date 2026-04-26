using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly AdminServiceDbContext _context;

    public MenuItemRepository(AdminServiceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<List<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        await _context.MenuItems.AddAsync(menuItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return menuItem;
    }

    public async Task<MenuItem> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync(cancellationToken);
        return menuItem;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems.FindAsync(new object[] { id }, cancellationToken);
        if (item == null) return false;
        
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
