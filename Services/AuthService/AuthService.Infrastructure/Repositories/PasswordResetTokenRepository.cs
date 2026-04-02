using System;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AuthDbContext _context;
    public PasswordResetTokenRepository(AuthDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task MarkUsedAsync(string tokenId)
    {
        var token = await _context.PasswordResetTokens.FindAsync(tokenId);
        if (token == null) return;
        token.IsUsed = true;
        await _context.SaveChangesAsync();
    }
}
