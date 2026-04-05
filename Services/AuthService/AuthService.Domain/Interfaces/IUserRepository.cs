using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(Guid UserId);
    Task<bool> CreateUserAsync(User user,string password);
    Task<bool> CheckPasswordAsync(Guid userId,string password);
    Task<bool> SetEmailVerifiedAsync(Guid userId);
    Task<bool> SetAccountStatusAsync(Guid userId, AccountStatus accountStatus);
    Task<bool> SetTwoFactorEnabledAsync(Guid userId, bool enabled);
    Task<bool> UpdatePasswordAsync(Guid userId, string newPassword);
    Task<bool> DeleteUserAsync(Guid userId);
}
