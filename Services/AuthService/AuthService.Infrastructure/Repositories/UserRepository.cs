using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthDbContext _context;
    public UserRepository(UserManager<ApplicationUser> userManager, AuthDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if(appUser==null) return false;
        return await _userManager.CheckPasswordAsync(appUser,password);
    }
    public async Task<bool> UpdatePasswordAsync(Guid userId, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return false;
        var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        if (result.Succeeded)
            await _context.SaveChangesAsync();
        return result.Succeeded;
    }

    public async Task<bool> CreateUserAsync(User user, string password)
    {
        ApplicationUser appUser = new ApplicationUser()
        {
            UserName = user.Email,
            Email = user.Email,
            FullName = user.FullName,
            UserName_Custom = user.UserName,
            MobileNumber = user.MobileNumber,
            Role = user.Role.ToString(),
            IsActive = true,
            IsEmailVerified = false,
            IsTwoFactorEnabled = false,
            AccountStatus = (int)user.AccountStatus,
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

    public async Task<User?> FindByIdAsync(Guid UserId)
    {
        var user = await _userManager.FindByIdAsync(UserId.ToString());
         if(user == null) return null;
         return MapToDomain(user);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToDomain).ToList();
    }

    public async Task<bool> SetEmailVerifiedAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return false;
        appUser.IsEmailVerified = true;
        var result = await _userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            await _context.SaveChangesAsync();
        return result.Succeeded;
    }

    public async Task<bool> SetAccountStatusAsync(Guid userId, AccountStatus accountStatus)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return false;
        appUser.AccountStatus = (int)accountStatus;
        var result = await _userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            await _context.SaveChangesAsync();
        return result.Succeeded;
    }

    public async Task<bool> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return false;
        appUser.IsTwoFactorEnabled = enabled;
        var result = await _userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            await _context.SaveChangesAsync();
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return false;
        
        appUser.FullName = user.FullName;
        appUser.MobileNumber = user.MobileNumber;
        
        var result = await _userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            await _context.SaveChangesAsync();
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string newPassword)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return false;
        var result = await _userManager.DeleteAsync(appUser);
        return result.Succeeded;
    }

    private User MapToDomain(ApplicationUser appUser)
    {
        return new User
        {
            Id = Guid.Parse(appUser.Id),
            FullName = appUser.FullName,
            UserName = appUser.UserName_Custom,
            Email = appUser.Email!,
            MobileNumber = appUser.MobileNumber,
            Role = Enum.Parse<AuthService.Domain.Enums.UserRole>(appUser.Role),
            IsActive = appUser.IsActive,
            IsEmailVerified = appUser.IsEmailVerified,
            IsTwoFactorVerified = appUser.IsTwoFactorEnabled,
            AccountStatus = (AuthService.Domain.Enums.AccountStatus)appUser.AccountStatus,
            ApprovedByAdminId = !string.IsNullOrWhiteSpace(appUser.ApprovedByAdminId) ? Guid.Parse(appUser.ApprovedByAdminId) : null,
            ApprovedAt = appUser.ApprovedAt,
            ApprovalNotes = appUser.ApprovalNotes,
            CreatedAt = appUser.CreatedAt
        };
    }

    public async Task<bool> IsAdminAsync(Guid userId)
    {
        var user = await FindByIdAsync(userId);
        return user != null && user.Role == UserRole.Admin;
    }

    public async Task<bool> IsUserAsync(Guid userId)
    {
        var user = await FindByIdAsync(userId);
        return user != null && user.Role == UserRole.Customer;
    }

    public async Task<bool> IsRestaurantAsync(Guid userId)
    {
        var user = await FindByIdAsync(userId);
        return user != null && user.Role == UserRole.RestaurantPartner;
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId)
    {
        var user = await FindByIdAsync(userId);
        return user?.Role;
    }
}
