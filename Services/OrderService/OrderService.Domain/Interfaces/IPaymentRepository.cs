namespace OrderService.Domain.Interfaces;

using OrderService.Domain.Entities;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}