using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface ITwoFactorTokenRepository
{
    Task<TwoFactorToken?> GetByTempTokenAsync(string tempToken);
    Task AddAsync(TwoFactorToken token);
    Task MarkUsedAsync(string tokenId);
}
