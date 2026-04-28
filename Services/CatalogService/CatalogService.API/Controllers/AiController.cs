using CatalogService.Application.DTOs.Ai;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Interact with the AI Assistant for smart recommendations
    /// </summary>
    [HttpPost("chat")]
    [AllowAnonymous]
    public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto request)
    {
        if (request == null || request.Messages == null || !request.Messages.Any())
        {
            return BadRequest("Messages cannot be empty.");
        }

        try
        {
            var result = await _aiService.GetChatResponseAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error processing AI request");
        }
    }
}
