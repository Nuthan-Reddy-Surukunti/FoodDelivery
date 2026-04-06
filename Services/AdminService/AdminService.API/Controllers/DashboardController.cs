using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Application.Interfaces;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get dashboard KPIs (orders, GMV, partners, SLA metrics)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboardKpis()
    {
        var kpis = await _dashboardService.GetKpisAsync();
        return Ok(kpis);
    }

    /// <summary>
    /// Get approval queue for pending restaurants
    /// </summary>
    [HttpGet("approval-queue")]
    public async Task<IActionResult> GetApprovalQueue()
    {
        var queue = await _dashboardService.GetApprovalQueueAsync();
        return Ok(queue);
    }
}
