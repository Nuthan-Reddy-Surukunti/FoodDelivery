using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for Report entity operations and analytics queries
/// </summary>
public interface IReportRepository : IRepository<object>
{
    Task<IEnumerable<object>> GetByTypeAsync(ReportType reportType, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ReportMetrics> GetSalesMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ReportMetrics> GetRestaurantPerformanceAsync(Guid restaurantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetOrderAnalyticsByStatusAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
