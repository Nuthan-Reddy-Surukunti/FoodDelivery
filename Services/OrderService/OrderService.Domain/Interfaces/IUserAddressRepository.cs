namespace OrderService.Domain.Interfaces;

using OrderService.Domain.Entities;

public interface IUserAddressRepository
{
    Task<UserAddress?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserAddress address, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserAddress address, CancellationToken cancellationToken = default);

    Task DeleteAsync(UserAddress address, CancellationToken cancellationToken = default);

    Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
