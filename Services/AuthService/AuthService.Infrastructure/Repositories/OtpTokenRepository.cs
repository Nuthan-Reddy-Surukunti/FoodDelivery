using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class OtpTokenRepository : IOtpTokenRepository
{
    private readonly AuthDbContext _dbContext;
    
    public OtpTokenRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OtpToken?> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.OtpTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<OtpToken?> GetByOtpCodeAsync(Guid userId, string otpCode)
    {
        return await _dbContext.OtpTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.OTP == otpCode && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task AddAsync(OtpToken otpToken)
    {
        await _dbContext.OtpTokens.AddAsync(otpToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(OtpToken otpToken)
    {
        _dbContext.OtpTokens.Update(otpToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid otpTokenId)
    {
        var token = await _dbContext.OtpTokens.FindAsync(otpTokenId);
        if (token != null)
        {
            _dbContext.OtpTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
        }
    }
}
