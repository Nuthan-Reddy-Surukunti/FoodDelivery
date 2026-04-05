using Microsoft.EntityFrameworkCore;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AdminServiceDbContext _context;

    public OrderRepository(AdminServiceDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<object>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.ToListAsync(cancellationToken);
    }

    public async Task<object> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        var order = (Order)entity;
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task UpdateAsync(object entity, CancellationToken cancellationToken = default)
    {
        var order = (Order)entity;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<object>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.Where(o => o.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<object>> GetDisputedOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.Status == OrderStatus.Disputed)
            .OrderBy(o => o.DisputeRaisedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<object> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<object>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<object>> GetOrdersByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.CountAsync(o => o.Status == status, cancellationToken);
    }
}
