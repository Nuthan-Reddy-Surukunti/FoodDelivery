namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Helpers;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

public class UserAddressService : IUserAddressService
{
    private readonly IUserAddressRepository _userAddressRepository;

    public UserAddressService(IUserAddressRepository userAddressRepository)
    {
        _userAddressRepository = userAddressRepository ?? throw new ArgumentNullException(nameof(userAddressRepository));
    }

    public async Task<IReadOnlyList<UserAddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));

        var addresses = await _userAddressRepository.GetByUserIdAsync(userId, cancellationToken);
        return addresses.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<UserAddressDto> GetUserAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(addressId, nameof(addressId));

        var address = await _userAddressRepository.GetByIdAsync(addressId, cancellationToken)
            ?? throw new ResourceNotFoundException("UserAddress", addressId);

        EnsureOwnership(userId, address);

        return MapToDto(address);
    }

    public async Task<UserAddressDto> CreateUserAddressAsync(Guid userId, CreateUserAddressRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ValidateCreateRequest(request);

        var existingAddresses = await _userAddressRepository.GetByUserIdAsync(userId, cancellationToken);

        var shouldBeDefault = request.IsDefault || !existingAddresses.Any();
        if (shouldBeDefault)
        {
            await UnsetDefaultAddressIfAnyAsync(userId, existingAddresses, cancellationToken);
        }

        var address = new UserAddress
        {
            UserId = userId,
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PostalCode = request.PostalCode.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AddressType = request.AddressType,
            IsDefault = shouldBeDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userAddressRepository.AddAsync(address, cancellationToken);

        return MapToDto(address);
    }

    public async Task<UserAddressDto> UpdateUserAddressAsync(Guid userId, Guid addressId, UpdateUserAddressRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(addressId, nameof(addressId));
        ServiceValidationHelper.ValidateNotNull(request, nameof(request));

        var address = await _userAddressRepository.GetByIdAsync(addressId, cancellationToken)
            ?? throw new ResourceNotFoundException("UserAddress", addressId);

        EnsureOwnership(userId, address);

        if (!string.IsNullOrWhiteSpace(request.AddressLine1))
        {
            address.AddressLine1 = request.AddressLine1.Trim();
        }

        if (request.AddressLine2 is not null)
        {
            address.AddressLine2 = string.IsNullOrWhiteSpace(request.AddressLine2) ? null : request.AddressLine2.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            address.City = request.City.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            address.State = request.State.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.PostalCode))
        {
            address.PostalCode = request.PostalCode.Trim();
        }

        if (request.Latitude.HasValue)
        {
            address.Latitude = request.Latitude;
        }

        if (request.Longitude.HasValue)
        {
            address.Longitude = request.Longitude;
        }

        if (request.AddressType.HasValue)
        {
            address.AddressType = request.AddressType.Value;
        }

        if (request.IsDefault == true)
        {
            var existingAddresses = await _userAddressRepository.GetByUserIdAsync(userId, cancellationToken);
            await UnsetDefaultAddressIfAnyAsync(userId, existingAddresses.Where(a => a.Id != address.Id), cancellationToken);
            address.IsDefault = true;
        }

        address.UpdatedAt = DateTime.UtcNow;
        await _userAddressRepository.UpdateAsync(address, cancellationToken);

        return MapToDto(address);
    }

    public async Task DeleteUserAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(addressId, nameof(addressId));

        var address = await _userAddressRepository.GetByIdAsync(addressId, cancellationToken)
            ?? throw new ResourceNotFoundException("UserAddress", addressId);

        EnsureOwnership(userId, address);

        var existingAddresses = await _userAddressRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingAddresses.Count <= 1)
        {
            throw new ValidationException("At least one address must be kept for ordering.");
        }

        await _userAddressRepository.DeleteAsync(address, cancellationToken);

        if (address.IsDefault)
        {
            var remaining = await _userAddressRepository.GetByUserIdAsync(userId, cancellationToken);
            var nextDefault = remaining.OrderByDescending(a => a.UpdatedAt).FirstOrDefault();
            if (nextDefault is not null && !nextDefault.IsDefault)
            {
                nextDefault.IsDefault = true;
                nextDefault.UpdatedAt = DateTime.UtcNow;
                await _userAddressRepository.UpdateAsync(nextDefault, cancellationToken);
            }
        }
    }

    private static void EnsureOwnership(Guid userId, UserAddress address)
    {
        if (address.UserId != userId)
        {
            throw new ValidationException("Address does not belong to the current user.");
        }
    }

    private static UserAddressDto MapToDto(UserAddress address)
    {
        return new UserAddressDto
        {
            AddressId = address.Id,
            UserId = address.UserId,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            AddressType = address.AddressType,
            IsDefault = address.IsDefault,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    private static void ValidateCreateRequest(CreateUserAddressRequestDto request)
    {
        ServiceValidationHelper.ValidateNotNull(request, nameof(request));
        ServiceValidationHelper.ValidateNotNullOrWhitespace(request.AddressLine1, nameof(request.AddressLine1));
        ServiceValidationHelper.ValidateNotNullOrWhitespace(request.City, nameof(request.City));
        ServiceValidationHelper.ValidateNotNullOrWhitespace(request.State, nameof(request.State));
        ServiceValidationHelper.ValidateNotNullOrWhitespace(request.PostalCode, nameof(request.PostalCode));
    }

    private async Task UnsetDefaultAddressIfAnyAsync(Guid userId, IEnumerable<UserAddress> addresses, CancellationToken cancellationToken)
    {
        foreach (var existing in addresses.Where(a => a.UserId == userId && a.IsDefault))
        {
            existing.IsDefault = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _userAddressRepository.UpdateAsync(existing, cancellationToken);
        }
    }
}
