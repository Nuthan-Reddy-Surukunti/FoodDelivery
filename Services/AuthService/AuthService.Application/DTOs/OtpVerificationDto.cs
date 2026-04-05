namespace AuthService.Application.DTOs;

public class OtpVerificationDto
{
    public Guid UserId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}
