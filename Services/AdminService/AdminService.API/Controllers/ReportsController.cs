using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminService.Application.Interfaces;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Generate sales report (PRD Required: GET /admin/reports/sales)
    /// </summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? restaurantId = null)
    {
        var report = await _reportService.GenerateSalesReportAsync(startDate, endDate, restaurantId);
        return Ok(report);
    }

    /// <summary>
    /// Generate partner performance report (PRD Required: GET /admin/reports/partners)
    /// </summary>
    [HttpGet("partners")]
    public async Task<IActionResult> GetPartnerPerformanceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? restaurantId = null)
    {
        var report = await _reportService.GeneratePartnerPerformanceReportAsync(startDate, endDate, restaurantId);
        return Ok(report);
    }

    /// <summary>
    /// Get reports by date range
    /// </summary>
    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetReportsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var reports = await _reportService.GetByDateRangeAsync(startDate, endDate);
        return Ok(reports);
    }

    /// <summary>
    /// Get specific report by ID
    /// </summary>
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
}
