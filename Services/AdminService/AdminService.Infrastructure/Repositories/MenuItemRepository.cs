using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for MenuItem operations
/// </summary>
public class MenuItemRepository : IMenuItemRepository
{
    private readonly AdminServiceDbContext _context;

    public MenuItemRepository(AdminServiceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.ToListAsync(cancellationToken);
    }

    public async Task<MenuItem> AddAsync(MenuItem entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Check for duplicate name within the same restaurant
        var duplicate = await ExistsWithNameAsync(entity.RestaurantId, entity.Name, cancellationToken: cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"Menu item with name '{entity.Name}' already exists for this restaurant");

        await _context.MenuItems.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(MenuItem entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Check for duplicate name within the same restaurant (excluding current item)
        var duplicate = await ExistsWithNameAsync(entity.RestaurantId, entity.Name, entity.Id, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"Menu item with name '{entity.Name}' already exists for this restaurant");

        _context.MenuItems.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems.FindAsync(new object[] { id }, cancellationToken);
        if (item != null)
        {
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.AnyAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<MenuItem> Items, int TotalCount)> GetPagedAsync(
        Guid? restaurantId = null,
        int pageNumber = 1,
        int pageSize = 10,
        MenuItemStatus? status = null,
        ApprovalStatus? approvalStatus = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems.AsQueryable();

        if (restaurantId.HasValue)
            query = query.Where(m => m.RestaurantId == restaurantId.Value);

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (approvalStatus.HasValue)
            query = query.Where(m => m.ApprovalStatus == approvalStatus.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> GetActiveByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && 
                       m.Status == MenuItemStatus.Active && 
                       m.ApprovalStatus == ApprovalStatus.Approved)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.ApprovalStatus == ApprovalStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(Guid restaurantId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && 
                       m.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetCountByApprovalStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.ApprovalStatus == status)
            .CountAsync(cancellationToken);
    }
}