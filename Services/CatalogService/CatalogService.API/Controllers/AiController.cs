using System.Security.Claims;
using CatalogService.Application.DTOs.Ai;
using CatalogService.Application.Interfaces;
using QuickBite.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
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
            _logger.LogError(ex, "AI chat request failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "INTERNAL_ERROR"
            });
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
