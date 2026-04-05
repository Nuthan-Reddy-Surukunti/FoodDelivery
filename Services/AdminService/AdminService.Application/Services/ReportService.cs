using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Domain.ValueObjects;

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

    public async Task<ReportDto> GenerateSalesReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        var metrics = await _reportRepository.GetSalesMetricsAsync(request.StartDate, request.EndDate, cancellationToken);
        var report = Report.Create(ReportType.Sales, metrics, request.StartDate, request.EndDate, request.FilterCriteria);
        
        var savedReport = await _reportRepository.AddAsync(report, cancellationToken);
        return _mapper.Map<ReportDto>(savedReport);
    }

    public async Task<ReportDto> GenerateUserAnalyticsAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        var metrics = await _reportRepository.GetUserAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
        var report = Report.Create(ReportType.UserAnalytics, metrics, request.StartDate, request.EndDate, request.FilterCriteria);
        
        var savedReport = await _reportRepository.AddAsync(report, cancellationToken);
        return _mapper.Map<ReportDto>(savedReport);
    }

    public async Task<ReportDto> GenerateRestaurantPerformanceAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.RestaurantId.HasValue)
            throw new ArgumentException("Restaurant ID is required for restaurant performance report");

        var metrics = await _reportRepository.GetRestaurantPerformanceAsync(request.RestaurantId.Value, request.StartDate, request.EndDate, cancellationToken);
        var report = Report.Create(ReportType.RestaurantPerformance, metrics, request.StartDate, request.EndDate, request.FilterCriteria);
        
        var savedReport = await _reportRepository.AddAsync(report, cancellationToken);
        return _mapper.Map<ReportDto>(savedReport);
    }

    public async Task<ReportDto> GenerateCustomReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        // For custom reports, we'll use sales metrics as a base
        var metrics = await _reportRepository.GetSalesMetricsAsync(request.StartDate, request.EndDate, cancellationToken);
        var report = Report.Create(ReportType.Custom, metrics, request.StartDate, request.EndDate, request.FilterCriteria);
        
        var savedReport = await _reportRepository.AddAsync(report, cancellationToken);
        return _mapper.Map<ReportDto>(savedReport);
    }
}
