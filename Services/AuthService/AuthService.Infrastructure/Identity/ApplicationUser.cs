using System;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Identity;

public class ApplicationUser:IdentityUser<string>
{
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsTwoFactorEnabled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
