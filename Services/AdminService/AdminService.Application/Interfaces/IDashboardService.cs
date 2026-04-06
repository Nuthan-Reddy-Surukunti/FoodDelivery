using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpisDto> GetKpisAsync(CancellationToken cancellationToken = default);
    Task<List<RestaurantDto>> GetApprovalQueueAsync(CancellationToken cancellationToken = default);
}