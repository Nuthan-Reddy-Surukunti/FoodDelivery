namespace OrderService.Application.Interfaces;

using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Requests;

public interface IUserAddressService
{
    Task<IReadOnlyList<UserAddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserAddressDto> GetUserAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);

    Task<UserAddressDto> CreateUserAddressAsync(Guid userId, CreateUserAddressRequestDto request, CancellationToken cancellationToken = default);

    Task<UserAddressDto> UpdateUserAddressAsync(Guid userId, Guid addressId, UpdateUserAddressRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteUserAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
