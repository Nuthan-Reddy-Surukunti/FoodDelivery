using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
        _logger = NullLogger<OrderRepository>.Instance;
    }

    // DI constructor used by the container
    public OrderRepository(OrderDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(order => order.OrderItems)
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment!)
                .ThenInclude(assignment => assignment.DeliveryAgent)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(order => order.OrderItems)
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment!)
                .ThenInclude(assignment => assignment.DeliveryAgent)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order {OrderId}. Payment present: {HasPayment}. DeliveryAssignment present: {HasDelivery}", order.Id, order.Payment != null, order.DeliveryAssignment != null);
        // Entities are already tracked when loaded via GetByIdAsync.
        // Ensure child entities are handled correctly so EF inserts new children
        // instead of attempting to UPDATE non-existent rows which causes
        // DbUpdateConcurrencyException.

        // Payment handling
        if (order.Payment != null)
        {
            var paymentEntry = _context.Entry(order.Payment);
            _logger.LogDebug("Payment entry state: {State}", paymentEntry.State);
            if (paymentEntry.State == EntityState.Detached)
            {
                var existingPayment = await _context.Payments.FindAsync(new object[] { order.Payment.Id }, cancellationToken);
                if (existingPayment == null)
                {
                    _logger.LogInformation("Adding new Payment for Order {OrderId} (PaymentId: {PaymentId})", order.Id, order.Payment.Id);
                    _context.Payments.Add(order.Payment);
                }
                else
                {
                    _logger.LogInformation("Updating existing Payment for Order {OrderId} (PaymentId: {PaymentId})", order.Id, order.Payment.Id);
                    _context.Payments.Update(order.Payment);
                }
            }
        }

        // DeliveryAssignment handling
        if (order.DeliveryAssignment != null)
        {
            var daEntry = _context.Entry(order.DeliveryAssignment);
            _logger.LogDebug("DeliveryAssignment entry state: {State}", daEntry.State);
            if (daEntry.State == EntityState.Detached)
            {
                var existingDa = await _context.DeliveryAssignments.FindAsync(new object[] { order.DeliveryAssignment.Id }, cancellationToken);
                if (existingDa == null)
                {
                    _logger.LogInformation("Adding new DeliveryAssignment for Order {OrderId} (AssignmentId: {AssignmentId})", order.Id, order.DeliveryAssignment.Id);
                    _context.DeliveryAssignments.Add(order.DeliveryAssignment);
                }
                else
                {
                    _logger.LogInformation("Updating existing DeliveryAssignment for Order {OrderId} (AssignmentId: {AssignmentId})", order.Id, order.DeliveryAssignment.Id);
                    _context.DeliveryAssignments.Update(order.DeliveryAssignment);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(order => order.OrderItems)
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment!)
                .ThenInclude(assignment => assignment.DeliveryAgent)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetActiveOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(order => order.OrderItems)
            .Include(order => order.Payment)
            .Include(order => order.DeliveryAssignment)
            .Where(order => order.OrderStatus != OrderStatus.Delivered &&
                            order.OrderStatus != OrderStatus.Refunded)
            .OrderByDescending(order => order.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(order => order.OrderItems)
            .Include(order => order.Payment)
            .Where(order => order.RestaurantId == restaurantId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
