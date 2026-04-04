using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task AddAsync(PasswordResetToken token);
    Task MarkUsedAsync(Guid tokenId);
}
