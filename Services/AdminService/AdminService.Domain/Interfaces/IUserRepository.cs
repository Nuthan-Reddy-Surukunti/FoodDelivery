using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IRepository<object>
{
    Task<object?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<(IEnumerable<object> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}
