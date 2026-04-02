using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetLatestByUserIdAsync(string userId);
    Task AddAsync(EmailVerificationToken token);
    Task MarkUsedAsync(string tokenId);
}
