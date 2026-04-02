using System;

namespace AuthService.Application.DTOs;

public class ToggleTwoFactorRequestDto
{
    public string UserId { get; set; }=string.Empty;
    public bool Enable { get; set; }
}
