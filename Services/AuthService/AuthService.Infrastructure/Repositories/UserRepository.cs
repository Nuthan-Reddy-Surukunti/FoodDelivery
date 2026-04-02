using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<bool> CheckPasswordAsync(string userId, string password)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if(appUser==null) return false;
        return await _userManager.CheckPasswordAsync(appUser,password);
    }
    public async Task<bool> UpdatePasswordAsync(string userId, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser == null) return false;
        var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> CreateUserAsync(User user, string password)
    {
        ApplicationUser appUser = new ApplicationUser()
        {
            UserName = user.Email,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsActive = true,
            IsEmailVerified = false,
            IsTwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow  
        };
        var result = await _userManager.CreateAsync(appUser,password);
        return result.Succeeded;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
         var user = await _userManager.FindByEmailAsync(email);
         if(user == null) return null;
         return MapToDomain(user);
    }

    public async Task<User?> FindByIdAsync(string UserId)
    {
        var user = await _userManager.FindByIdAsync(UserId);
         if(user == null) return null;
         return MapToDomain(user);
    }

    public async Task<bool> SetEmailVerifiedAsync(string userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser == null) return false;
        appUser.IsEmailVerified = true;
        var result = await _userManager.UpdateAsync(appUser);
        return result.Succeeded;
    }

    public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser == null) return false;
        appUser.IsTwoFactorEnabled = enabled;
        var result = await _userManager.UpdateAsync(appUser);
        return result.Succeeded;
    }
    private User MapToDomain(ApplicationUser appUser)
    {
        return new User
        {
            Id = appUser.Id,
            FullName = appUser.FullName,
            Email = appUser.Email!,
            Role = Enum.Parse<AuthService.Domain.Enums.UserRole>(appUser.Role),
            IsActive = appUser.IsActive,
            IsEmailVerified = appUser.IsEmailVerified,
            IsTwoFactorVerified = appUser.IsTwoFactorEnabled,
            CreatedAt = appUser.CreatedAt
        };
    }
}
