using AdminService.Domain.Enums;
using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repository interface for Report entity operations and analytics queries
/// </summary>
public interface IReportRepository : IRepository<Report>
{
    Task<IEnumerable<Report>> GetByTypeAsync(ReportType reportType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Report>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<(int TotalOrders, decimal TotalRevenue, string Currency, int TotalCustomers, int TotalRestaurants, double AverageOrderValue)> GetSalesMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetUserRegistrationAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetRestaurantAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
