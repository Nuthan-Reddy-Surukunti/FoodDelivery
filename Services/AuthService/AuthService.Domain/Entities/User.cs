using System;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{
    public string Id { get; set; }=string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; }=string.Empty;
    public string PasswordHash { get; set; }=string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }=false;
    public bool IsEmailVerified { get; set; }=false;
    public bool IsTwoFactorVerified { get; set; }=false;
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
}
