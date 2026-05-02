using OrderService.Application.DTOs.Profile;

namespace OrderService.Application.Interfaces;

public interface IProfileStatsService
{
    Task<ProfileStatsDto> GetProfileStatsAsync(Guid userId, string role, CancellationToken cancellationToken = default);
}
