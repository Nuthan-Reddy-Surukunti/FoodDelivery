using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

public interface IRestaurantService
{
    Task<RestaurantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<RestaurantDto>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<RestaurantDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<RestaurantDto> ApproveAsync(Guid id, ApproveRestaurantRequest request, CancellationToken cancellationToken = default);
    Task<RestaurantDto> RejectAsync(Guid id, RejectRestaurantRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RestaurantDto>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
