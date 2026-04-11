namespace AuthService.Application.DTOs;

/// <summary>
/// DTO for verifying OTP on RestaurantPartner/Admin first-login after admin approval
/// </summary>
public class FirstLoginVerificationRequestDto
{
    /// <summary>
    /// The user ID (RestaurantPartner or Admin being verified)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The OTP code sent to user's email
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
}
