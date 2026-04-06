using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminService.Application.Services;
using AdminService.Application.DTOs.Requests;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(Guid id)
    {
        try
        {
            var report = await _reportService.GetByIdAsync(id);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetReportsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var reports = await _reportService.GetByDateRangeAsync(startDate, endDate);
        return Ok(reports);
    }

    [HttpPost("sales")]
    public async Task<IActionResult> GenerateSalesReport([FromBody] GenerateReportRequest request)
    {
        var report = await _reportService.GenerateSalesReportAsync(request);
        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }

    [HttpPost("user-analytics")]
    public async Task<IActionResult> GenerateUserAnalytics([FromBody] GenerateReportRequest request)
    {
        var report = await _reportService.GenerateUserAnalyticsAsync(request);
        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }

    [HttpPost("restaurant-performance")]
    public async Task<IActionResult> GenerateRestaurantPerformance([FromBody] GenerateReportRequest request)
    {
        var report = await _reportService.GenerateRestaurantPerformanceAsync(request);
        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }

    [HttpPost("custom")]
    public async Task<IActionResult> GenerateCustomReport([FromBody] GenerateReportRequest request)
    {
        var report = await _reportService.GenerateCustomReportAsync(request);
        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }
}
