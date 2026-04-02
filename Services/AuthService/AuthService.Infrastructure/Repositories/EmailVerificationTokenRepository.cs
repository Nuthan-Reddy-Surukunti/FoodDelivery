using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AuthDbContext _context;
    public EmailVerificationTokenRepository(AuthDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(EmailVerificationToken token)
    {
        await _context.EmailVerificationTokens.AddAsync(token);
    }

    public async Task<EmailVerificationToken?> GetLatestByUserIdAsync(string userId)
    {
        return await _context.EmailVerificationTokens
        .Where(i=>i.UserId==userId)
        .OrderByDescending(c=>c.CreatedAt).FirstOrDefaultAsync();

    }

    public async Task MarkUsedAsync(string tokenId)
    {
        var token = await _context.EmailVerificationTokens.FirstOrDefaultAsync(i=>i.Id==tokenId);
        if(token==null) return;
        token.IsUsed=true;
        await _context.SaveChangesAsync();
    }
}
