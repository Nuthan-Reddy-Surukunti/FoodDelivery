using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IMapper _mapper;

    public ReportService(IReportRepository reportRepository, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _mapper = mapper;
    }

    public async Task<ReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID {id} not found");

        return _mapper.Map<ReportDto>(report);
    }

    public async Task<IEnumerable<ReportDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var reports = await _reportRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return _mapper.Map<IEnumerable<ReportDto>>(reports);
    }

    public async Task<ReportDto> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var metrics = await _reportRepository.GetSalesMetricsAsync(startDate, endDate, cancellationToken);
        var report = new Report
        {
            Id = Guid.NewGuid(),
            Type = ReportType.Sales,
            TotalOrders = metrics.TotalOrders,
            TotalRevenue = metrics.TotalRevenue,
            Currency = metrics.Currency,
            TotalCustomers = metrics.TotalCustomers,
            TotalRestaurants = metrics.TotalRestaurants,
            AverageOrderValue = metrics.AverageOrderValue,
            MetricsStartDate = startDate,
            MetricsEndDate = endDate,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };
        
        var savedReport = await _reportRepository.AddAsync(report, cancellationToken);
        return _mapper.Map<ReportDto>(savedReport);
    }

    public async Task<UserAnalyticsDto> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var analytics = await _reportRepository.GetUserRegistrationAnalyticsAsync(startDate, endDate, cancellationToken);
        
        var totalRegistrations = (int)(analytics.ContainsKey("TotalRegistrations") ? analytics["TotalRegistrations"] : 0);
        var activeUsers = (int)(analytics.ContainsKey("ActiveUsers") ? analytics["ActiveUsers"] : 0);
        var usersByRole = analytics.ContainsKey("UsersByRole") 
            ? (Dictionary<string, int>)analytics["UsersByRole"] 
            : new Dictionary<string, int> { { "Customer", totalRegistrations } };

        var dto = new UserAnalyticsDto
        {
            TotalUsersRegistered = totalRegistrations,
            ActiveUsers = activeUsers,
            UsersByRole = usersByRole,
            RegistrationTrend = new List<RegistrationTrendDto>
            {
                new RegistrationTrendDto { Date = startDate, NewRegistrations = totalRegistrations }
            },
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };

        return dto;
    }

    public async Task<RestaurantAnalyticsDto> GetRestaurantAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var analytics = await _reportRepository.GetRestaurantAnalyticsAsync(startDate, endDate, cancellationToken);
        
        var totalRestaurants = (int)(analytics.ContainsKey("TotalRestaurants") ? analytics["TotalRestaurants"] : 0);
        var pendingApprovals = (int)(analytics.ContainsKey("PendingApprovals") ? analytics["PendingApprovals"] : 0);
        var approvedCount = (int)(analytics.ContainsKey("ApprovedCount") ? analytics["ApprovedCount"] : 0);
        var totalRevenue = (decimal)(analytics.ContainsKey("TotalRevenue") ? analytics["TotalRevenue"] : 0m);
        var revenueByRestaurant = analytics.ContainsKey("RevenueByRestaurant") 
            ? (Dictionary<Guid, decimal>)analytics["RevenueByRestaurant"] 
            : new Dictionary<Guid, decimal>();

        var dto = new RestaurantAnalyticsDto
        {
            TotalRestaurants = totalRestaurants,
            PendingApprovals = pendingApprovals,
            ApprovedCount = approvedCount,
            TotalRevenue = totalRevenue,
            Currency = "USD",
            RevenueByRestaurant = revenueByRestaurant,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };

        return dto;
    }

    public async Task<ReportDto> GeneratePartnerPerformanceReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var metrics = await _reportRepository.GetSalesMetricsAsync(startDate, endDate, cancellationToken);
        var report = new Report
        {
            Id = Guid.NewGuid(),
            Type = ReportType.RestaurantPerformance,
            TotalOrders = metrics.TotalOrders,
            TotalRevenue = metrics.TotalRevenue,
            Currency = metrics.Currency,
            TotalCustomers = metrics.TotalCustomers,
            TotalRestaurants = metrics.TotalRestaurants,
            AverageOrderValue = metrics.AverageOrderValue,
            MetricsStartDate = startDate,
            MetricsEndDate = endDate,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };
        
        var savedReport = await _reportRepository.AddAsync(report, cancellationToken);
        return _mapper.Map<ReportDto>(savedReport);
    }
}
