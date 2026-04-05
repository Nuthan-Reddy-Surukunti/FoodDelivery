namespace AuthService.Application.Interfaces;

/// <summary>
/// Service for generating and verifying OTP codes
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new OTP code and stores it with expiration
    /// </summary>
    Task<bool> GenerateAndStoreOtpAsync(Guid userId);
    
    /// <summary>
    /// Verifies an OTP code and marks it as used if valid
    /// </summary>
    Task<bool> VerifyOtpAsync(Guid userId, string otpCode);
}
