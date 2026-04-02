using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class TwoFactorTokenRepository : ITwoFactorTokenRepository
{
    private readonly AuthDbContext _context;
    public TwoFactorTokenRepository(AuthDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(TwoFactorToken token)
    {
        await _context.TwoFactorTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<TwoFactorToken?> GetByTempTokenAsync(string tempToken)
    {
        return await _context.TwoFactorTokens.FirstOrDefaultAsync(i=>i.TempToken==tempToken);
    }

    public async Task MarkUsedAsync(string tokenId)
    {
        var token = await _context.TwoFactorTokens.FirstOrDefaultAsync(i=>i.Id==tokenId);
        if(token==null) return;
        token.IsUsed=true;
        await _context.SaveChangesAsync();
    }
}
