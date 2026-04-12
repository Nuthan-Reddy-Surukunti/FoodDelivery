using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDeliveryAgentSyncService _deliveryAgentSyncService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IDeliveryAgentSyncService deliveryAgentSyncService, ILogger<AdminController> logger)
    {
        _deliveryAgentSyncService = deliveryAgentSyncService ?? throw new ArgumentNullException(nameof(deliveryAgentSyncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Manually sync all delivery agents from AuthService to OrderService database.
    /// This endpoint is useful for syncing agents created before the automatic event handler was in place.
    /// Admin-only endpoint.
    /// </summary>
    [HttpPost("sync-delivery-agents")]
    public async Task<IActionResult> SyncDeliveryAgents(CancellationToken cancellationToken)
    {
        try
        {
            var syncedCount = await _deliveryAgentSyncService.SyncDeliveryAgentsFromAuthServiceAsync(cancellationToken);
            _logger.LogInformation("Admin-triggered delivery agent sync completed. Synced count: {SyncedCount}", syncedCount);
            
            return Ok(new
            {
                success = true,
                message = $"Successfully synced {syncedCount} delivery agent(s) from AuthService.",
                syncedCount = syncedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during delivery agent sync");
            return BadRequest(new
            {
                success = false,
                message = "Failed to sync delivery agents. Please check logs for details.",
                error = ex.Message
            });
        }
    }
}
