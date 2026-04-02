using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;
    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(i=>i.Token==token);
    }

    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(i=>i.Token==token);
        if(refreshToken==null) return;
        refreshToken.IsRevoked=true;
        await _context.SaveChangesAsync();
    }
}
