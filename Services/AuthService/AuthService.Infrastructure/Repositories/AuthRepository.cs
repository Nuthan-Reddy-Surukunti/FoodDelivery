using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthDbContext _context;

    public AuthRepository(UserManager<ApplicationUser> userManager, AuthDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IEnumerable<User>> GetPendingApprovalRestaurantsAsync()
    {
        var restaurants = await _context.Users
            .Where(u => u.Role == UserRole.RestaurantPartner.ToString() && 
                        u.AccountStatus == (int)AccountStatus.Pending)
            .ToListAsync();

        return restaurants.Select(MapToDomain).ToList();
    }

    public async Task<bool> ApproveRestaurantAsync(Guid restaurantId, Guid adminId, string? notes)
    {
        var restaurant = await _userManager.FindByIdAsync(restaurantId.ToString());
        if (restaurant == null)
        {
            Console.WriteLine($"ApproveRestaurantAsync: Restaurant not found for ID {restaurantId}");
            return false;
        }
        if (restaurant.Role != UserRole.RestaurantPartner.ToString())
        {
            Console.WriteLine($"ApproveRestaurantAsync: Role mismatch for ID {restaurantId}. Expected RestaurantPartner, got {restaurant.Role}");
            return false;
        }

        restaurant.AccountStatus = (int)AccountStatus.Active;
        restaurant.ApprovedByAdminId = adminId.ToString();
        restaurant.ApprovedAt = DateTime.UtcNow;
        restaurant.ApprovalNotes = notes;

        var result = await _userManager.UpdateAsync(restaurant);
        if (result.Succeeded)
        {
            await _context.SaveChangesAsync();
            return true;
        }

        Console.WriteLine($"UpdateAsync failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        return false;
    }

    public async Task<bool> RejectRestaurantAsync(Guid restaurantId, Guid adminId, string? notes)
    {
        var restaurant = await _userManager.FindByIdAsync(restaurantId.ToString());
        if (restaurant == null || restaurant.Role != UserRole.RestaurantPartner.ToString())
            return false;

        restaurant.AccountStatus = (int)AccountStatus.Rejected;
        restaurant.ApprovedByAdminId = adminId.ToString();
        restaurant.ApprovedAt = DateTime.UtcNow;
        restaurant.ApprovalNotes = notes;

        var result = await _userManager.UpdateAsync(restaurant);
        if (result.Succeeded)
            await _context.SaveChangesAsync();

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
            Role = Enum.Parse<UserRole>(appUser.Role),
            IsActive = appUser.IsActive,
            IsEmailVerified = appUser.IsEmailVerified,
            IsTwoFactorVerified = appUser.IsTwoFactorEnabled,
            AccountStatus = (AccountStatus)appUser.AccountStatus,
            ApprovedByAdminId = !string.IsNullOrWhiteSpace(appUser.ApprovedByAdminId) ? Guid.Parse(appUser.ApprovedByAdminId) : null,
            ApprovedAt = appUser.ApprovedAt,
            ApprovalNotes = appUser.ApprovalNotes,
            CreatedAt = appUser.CreatedAt
        };
    }
}
