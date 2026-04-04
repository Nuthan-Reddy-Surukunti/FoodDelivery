using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetLatestByUserIdAsync(Guid userId);
    Task AddAsync(EmailVerificationToken token);
    Task MarkUsedAsync(Guid tokenId);
}
