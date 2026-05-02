using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/profile/stats")]
[Authorize]
public class ProfileStatsController : ControllerBase
{
    private readonly IProfileStatsService _profileStatsService;

    public ProfileStatsController(IProfileStatsService profileStatsService)
    {
        _profileStatsService = profileStatsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyStats(CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var role = this.GetCurrentUserRole();

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var stats = await _profileStatsService.GetProfileStatsAsync(userId, role, cancellationToken);
        return Ok(stats);
    }
}
