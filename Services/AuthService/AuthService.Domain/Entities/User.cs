using System;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }=Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string? UserName { get; set; }

    public string Email { get; set; }=string.Empty;
    public string PasswordHash { get; set; }=string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }=false;
    public bool IsEmailVerified { get; set; }=false;
    public bool IsTwoFactorVerified { get; set; }=false;
    
    /// <summary>
    /// Account approval and verification status
    /// </summary>
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Verified;
    
    /// <summary>
    /// ID of the admin who approved this account (for RestaurantPartner/Admin)
    /// </summary>
    public Guid? ApprovedByAdminId { get; set; }
    
    /// <summary>
    /// Timestamp when the account was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Admin's notes on the approval/rejection
    /// </summary>
    public string? ApprovalNotes { get; set; }
    
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
}
