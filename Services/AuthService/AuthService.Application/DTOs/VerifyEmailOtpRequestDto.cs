using System;

namespace AuthService.Application.DTOs;

public class VerifyEmailOtpRequestDto
{
    public string Email { get; set; }=string.Empty;
    public string Otp { get; set; }=string.Empty;
}
