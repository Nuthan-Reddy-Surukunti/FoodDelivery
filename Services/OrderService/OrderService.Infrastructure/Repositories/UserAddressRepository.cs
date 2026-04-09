using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly OrderDbContext _context;

    public UserAddressRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<UserAddress?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(address => address.Id == addressId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .Where(address => address.UserId == userId)
            .OrderByDescending(address => address.IsDefault)
            .ThenByDescending(address => address.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserAddress address, CancellationToken cancellationToken = default)
    {
        _context.UserAddresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserAddress address, CancellationToken cancellationToken = default)
    {
        _context.UserAddresses.Update(address);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserAddress address, CancellationToken cancellationToken = default)
    {
        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(address => address.UserId == userId && address.IsDefault, cancellationToken);
    }
}
