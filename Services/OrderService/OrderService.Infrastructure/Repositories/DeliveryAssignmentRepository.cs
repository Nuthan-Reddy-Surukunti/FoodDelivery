using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class DeliveryAssignmentRepository : IDeliveryAssignmentRepository
{
    private readonly OrderDbContext _context;

    public DeliveryAssignmentRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeliveryAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.DeliveryAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeliveryAssignment?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAssignments
            .FirstOrDefaultAsync(assignment => assignment.Id == assignmentId, cancellationToken);
    }

    public async Task<DeliveryAssignment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAssignments
            .FirstOrDefaultAsync(assignment => assignment.OrderId == orderId, cancellationToken);
    }

    public async Task UpdateAsync(DeliveryAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.DeliveryAssignments.Update(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryAssignment>> GetAssignmentsByAgentIdAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAssignments
            .Where(assignment => assignment.DeliveryAgentId == deliveryAgentId)
            .OrderByDescending(assignment => assignment.AssignedAt)
            .ToListAsync(cancellationToken);
    }
}
