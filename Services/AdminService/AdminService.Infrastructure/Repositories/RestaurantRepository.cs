using Microsoft.EntityFrameworkCore;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Infrastructure.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly AdminServiceDbContext _context;

    public RestaurantRepository(AdminServiceDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<object>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.ToListAsync(cancellationToken);
    }

    public async Task<object> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        var restaurant = (Restaurant)entity;
        await _context.Restaurants.AddAsync(restaurant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return restaurant;
    }

    public async Task UpdateAsync(object entity, CancellationToken cancellationToken = default)
    {
        var restaurant = (Restaurant)entity;
        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants.FindAsync(new object[] { id }, cancellationToken);
        if (restaurant != null)
        {
            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<object>> GetByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.Where(r => r.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<object>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<object> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, RestaurantStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Restaurants.AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetCountByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.CountAsync(r => r.Status == status, cancellationToken);
    }

    public async Task ApproveRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants.FindAsync(new object[] { restaurantId }, cancellationToken);
        if (restaurant != null)
        {
            restaurant.Approve();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RejectRestaurantAsync(Guid restaurantId, string reason, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants.FindAsync(new object[] { restaurantId }, cancellationToken);
        if (restaurant != null)
        {
            restaurant.Reject(reason);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<object>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Where(r => r.OwnerId == ownerId && r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants.FindAsync(new object[] { restaurantId }, cancellationToken);
        if (restaurant != null)
        {
            restaurant.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
