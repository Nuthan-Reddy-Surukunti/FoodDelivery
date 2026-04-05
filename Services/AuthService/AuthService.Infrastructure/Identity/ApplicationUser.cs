using System;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Identity;

public class ApplicationUser:IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? UserName_Custom { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsTwoFactorEnabled { get; set; } = false;
    
    /// <summary>
    /// Account approval and verification status (0=Pending, 1=Active, 2=Verified, 3=Rejected)
    /// </summary>
    public int AccountStatus { get; set; } = 2;
    
    /// <summary>
    /// ID of the admin who approved this account
    /// </summary>
    public string? ApprovedByAdminId { get; set; }
    
    /// <summary>
    /// Timestamp when the account was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Admin's notes on the approval/rejection
    /// </summary>
    public string? ApprovalNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

