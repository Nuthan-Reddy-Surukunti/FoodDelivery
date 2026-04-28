using System.Security.Claims;
using CatalogService.Application.DTOs.Ai;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Interact with the QuickBite AI Assistant for smart recommendations and order tracking.
    /// </summary>
    [HttpPost("chat")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto request)
    {
        if (request == null || request.Messages == null || !request.Messages.Any())
            return BadRequest("Messages cannot be empty.");

        // Extract authenticated user identity from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = Guid.TryParse(userIdClaim, out var uid) ? uid : null;

        // Forward the bearer token so AiService can call OrderService on behalf of the user
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var authToken  = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authHeader[7..]
            : null;

        try
        {
            var result = await _aiService.GetChatResponseAsync(request, userId, authToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log the real exception so we can see what's going wrong
            Console.Error.WriteLine($"[AiController] Chat error: {ex.GetType().Name} — {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error processing AI request.");
        }
    }

    /// <summary>
    /// Lightweight status check — tells the frontend whether Gemini is reachable.
    /// Returns { online: true/false }
    /// </summary>
    [HttpGet("status")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> GetStatus()
    {
        var online = await _aiService.CheckConnectivityAsync();
        return Ok(new { online });
    }
}
