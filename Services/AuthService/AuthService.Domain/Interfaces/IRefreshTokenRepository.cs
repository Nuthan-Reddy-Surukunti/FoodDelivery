using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken token);
    Task RevokeAsync(string token);
}
