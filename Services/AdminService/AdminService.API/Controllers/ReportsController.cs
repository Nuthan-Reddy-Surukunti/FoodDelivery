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
    /// Get all users analytics (no date filters)
    /// </summary>
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUsersAnalytics()
    {
        var report = await _reportService.GetAllUsersAnalyticsAsync();
        return Ok(report);
    }

    /// <summary>
    /// Get all restaurants analytics (no date filters)
    /// </summary>
    [HttpGet("all-restaurants")]
    public async Task<IActionResult> GetAllRestaurantsAnalytics()
    {
        var report = await _reportService.GetAllRestaurantsAnalyticsAsync();
        return Ok(report);
    }

    /// <summary>
    /// Get all sales data (no date filters)
    /// </summary>
    [HttpGet("all-sales")]
    public async Task<IActionResult> GetAllSales()
    {
        var report = await _reportService.GetAllSalesAsync();
        return Ok(report);
    }

    /// <summary>
    /// Get all partners analytics (no date filters)
    /// </summary>
    [HttpGet("all-partners")]
    public async Task<IActionResult> GetAllPartners()
    {
        var report = await _reportService.GetAllPartnersAsync();
        return Ok(report);
    }
}
