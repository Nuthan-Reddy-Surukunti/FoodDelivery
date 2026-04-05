using System;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Identity;
using Azure.Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<TwoFactorToken> TwoFactorTokens { get; set; }
    public DbSet<OtpToken> OtpTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity=>
        {
            entity.HasKey(i=>i.Id);
            entity.Property(i=>i.Token).IsRequired();
        });
        builder.Entity<PasswordResetToken>(entity=>
        {
            entity.HasKey(i=>i.Id);
            entity.Property(i=>i.Token).IsRequired();
        });
        builder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.OTP).IsRequired();
        });
        builder.Entity<TwoFactorToken>(entity =>
        {
            entity.HasKey(t=>t.Id);
            entity.Property(i=>i.OTP).IsRequired();
            entity.Property(i=>i.TempToken).IsRequired();
        });
        
        builder.Entity<OtpToken>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.OTP).IsRequired();
            entity.Property(t => t.ExpiresAt).IsRequired();
        });

        // Seed initial admin user
        SeedAdminUser(builder);
    }

    private static void SeedAdminUser(ModelBuilder builder)
    {
        // This is a placeholder for seeding
        // In production, use a more secure approach or create an admin setup endpoint
        // Admin will be created via migrations or database initialization
    }