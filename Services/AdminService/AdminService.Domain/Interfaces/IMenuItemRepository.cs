using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<MenuItem> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
    Task<MenuItem> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
