using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

public interface IReportService
{
    Task<ReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ReportDto> GenerateSalesReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
    Task<ReportDto> GenerateUserAnalyticsAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
    Task<ReportDto> GenerateRestaurantPerformanceAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
    Task<ReportDto> GenerateCustomReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
}
