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

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(entity);
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

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.Where(o => o.Status == status).ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default)
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

    public async Task<int> GetTotalOrdersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.CountAsync(cancellationToken);
    }

    public async Task<(decimal Amount, string Currency)> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders.ToListAsync(cancellationToken);
        if (!orders.Any())
            return (0m, "INR");

        decimal totalAmount = orders.Sum(o => o.TotalAmount);
        var firstCurrency = string.IsNullOrWhiteSpace(orders.First().Currency) ? "INR" : orders.First().Currency;
        
        return (totalAmount, firstCurrency);
    }

    public async Task<int> GetOrdersCountByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
            .CountAsync(cancellationToken);
    }

    public async Task<(decimal Amount, string Currency)> GetRevenueBetweenDatesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
            .ToListAsync(cancellationToken);
            
        if (!orders.Any())
            return (0m, "INR");

        decimal totalAmount = orders.Sum(o => o.TotalAmount);
        var firstCurrency = string.IsNullOrWhiteSpace(orders.First().Currency) ? "INR" : orders.First().Currency;
        
        return (totalAmount, firstCurrency);
    }
}
