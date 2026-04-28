using CatalogService.Application.DTOs.Ai;

namespace CatalogService.Application.Interfaces;

public interface IAiService
{
    /// <summary>
    /// Process a chat request through Gemini with full function-calling loop.
    /// </summary>
    /// <param name="request">The chat messages from the frontend.</param>
    /// <param name="userId">Authenticated user's ID (null for guests).</param>
    /// <param name="authToken">JWT bearer token to forward to OrderService (null for guests).</param>
    Task<AiChatResponseDto> GetChatResponseAsync(
        AiChatRequestDto request,
        Guid? userId = null,
        string? authToken = null);

    /// <summary>Quick Gemini connectivity check for the status indicator.</summary>
    Task<bool> CheckConnectivityAsync();
}
