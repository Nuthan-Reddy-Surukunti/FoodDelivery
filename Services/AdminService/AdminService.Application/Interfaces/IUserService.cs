using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
