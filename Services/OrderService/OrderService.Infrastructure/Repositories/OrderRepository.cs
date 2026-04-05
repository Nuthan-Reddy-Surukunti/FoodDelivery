using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include("_orderItems")
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include("_orderItems")
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersReadyForDeliveryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include("_orderItems")
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment)
            .Where(order => order.OrderStatus == OrderStatus.ReadyForPickup)
            .OrderBy(order => order.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include("_orderItems")
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetActiveOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include("_orderItems")
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment)
            .Where(order => order.OrderStatus != OrderStatus.Delivered &&
                            order.OrderStatus != OrderStatus.Refunded)
            .OrderByDescending(order => order.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}
