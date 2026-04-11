using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

public interface IReportService
{
    Task<ReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // GetAll endpoints - no date filters, returns all data
    Task<UserAnalyticsDto> GetAllUsersAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<RestaurantAnalyticsDto> GetAllRestaurantsAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<ReportDto> GetAllSalesAsync(CancellationToken cancellationToken = default);
    Task<ReportDto> GetAllPartnersAsync(CancellationToken cancellationToken = default);
}
