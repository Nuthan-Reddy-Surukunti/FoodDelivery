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
    /// Generate sales report (PRD: GET /admin/reports/sales)
    /// </summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _reportService.GenerateSalesReportAsync(startDate, endDate);
        return Ok(report);
    }

    /// <summary>
    /// Generate partner performance report (PRD: GET /admin/reports/partners)
    /// </summary>
    [HttpGet("partners")]
    public async Task<IActionResult> GetPartnerPerformanceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _reportService.GeneratePartnerPerformanceReportAsync(startDate, endDate);
        return Ok(report);
    }
}
