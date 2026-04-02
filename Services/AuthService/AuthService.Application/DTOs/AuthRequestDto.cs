using System;

namespace AuthService.Application.DTOs;

public class AuthRequestDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Role { get; set; }
    public bool IsTwoFactorRequired { get; set; }=false;
    public string? UserId { get; set; }
    public string? TempToken { get; set; }
}
