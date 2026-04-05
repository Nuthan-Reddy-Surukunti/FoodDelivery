namespace AuthService.Domain.Entities;

/// <summary>
/// Represents an OTP token for email or login verification
/// </summary>
public class OtpToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The user who this OTP is for
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The 6-digit OTP code
    /// </summary>
    public string OTP { get; set; } = string.Empty;
    
    /// <summary>
    /// Expiration time for this OTP
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Whether this OTP has been used
    /// </summary>
    public bool IsUsed { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this OTP was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
}
