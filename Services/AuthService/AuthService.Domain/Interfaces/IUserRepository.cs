using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(string UserId);
    Task<bool> CreateUserAsync(User user,string password);
    Task<bool> CheckPasswordAsync(string userId,string password);
    Task<bool> SetEmailVerifiedAsync(string userId);
    Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled);
    Task<bool> UpdatePasswordAsync(string userId, string newPassword);
}
