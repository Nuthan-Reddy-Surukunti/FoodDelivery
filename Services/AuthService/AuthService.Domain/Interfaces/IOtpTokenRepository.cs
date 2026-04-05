using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IOtpTokenRepository
{
    Task<OtpToken?> GetByUserIdAsync(Guid userId);
    Task<OtpToken?> GetByOtpCodeAsync(Guid userId, string otpCode);
    Task AddAsync(OtpToken otpToken);
    Task UpdateAsync(OtpToken otpToken);
    Task DeleteAsync(Guid otpTokenId);
}
