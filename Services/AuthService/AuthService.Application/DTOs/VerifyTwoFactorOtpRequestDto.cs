using System;

namespace AuthService.Application.DTOs;

public class VerifyTwoFactorOtpRequestDto
{
    public int UserId { get; set; }
    public string Otp { get; set; }=string.Empty;
    public string TempToken { get; set; }=string.Empty;
}
