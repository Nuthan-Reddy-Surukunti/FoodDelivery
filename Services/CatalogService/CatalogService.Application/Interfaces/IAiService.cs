using CatalogService.Application.DTOs.Ai;

namespace CatalogService.Application.Interfaces;

public interface IAiService
{
    Task<AiChatResponseDto> GetChatResponseAsync(AiChatRequestDto request);
}
