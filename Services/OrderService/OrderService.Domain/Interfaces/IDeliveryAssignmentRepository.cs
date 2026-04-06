namespace OrderService.Domain.Interfaces;

using OrderService.Domain.Entities;

public interface IDeliveryAssignmentRepository
{
    Task AddAsync(DeliveryAssignment assignment, CancellationToken cancellationToken = default);

    Task<DeliveryAssignment?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<DeliveryAssignment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task UpdateAsync(DeliveryAssignment assignment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeliveryAssignment>> GetAssignmentsByAgentIdAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default);
}
