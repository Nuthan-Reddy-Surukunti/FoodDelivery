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

    public async Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Restaurant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Restaurant>> GetAllAsync(RestaurantStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Restaurants.AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var items = await query
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<Restaurant> AddAsync(Restaurant entity, CancellationToken cancellationToken = default)
    {
        await _context.Restaurants.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Restaurant entity, CancellationToken cancellationToken = default)
    {
        _context.Restaurants.Update(entity);
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

    public async Task<IEnumerable<Restaurant>> GetByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.Where(r => r.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Restaurant>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }


    public async Task<int> GetCountByStatusAsync(RestaurantStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants.CountAsync(r => r.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
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
            restaurant.IsActive = false;
            restaurant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
