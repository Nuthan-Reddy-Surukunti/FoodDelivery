using System;
using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<AuthRequestDto> VerifyEmailOtpAsync(VerifyEmailOtpRequestDto request);
    Task<AuthRequestDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthRequestDto> LoginAsync(LoginRequestDto request);
    Task<AuthRequestDto> VerifyTwoFactorOtpAsync(VerifyTwoFactorOtpRequestDto request);
    Task<AuthRequestDto> RefreshTokenAsync(RefreshTokenDto request);
    Task<AuthRequestDto> LogoutAsync(RefreshTokenDto dto);
    Task<AuthRequestDto> ForgotPasswordAsync(ForgotPasswordDto request);
    Task<AuthRequestDto> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<AuthRequestDto> ToggleTwoFactorAsync(string userId, ToggleTwoFactorRequestDto request);
    Task<AuthRequestDto> DeleteUserAsync(DeleteUserRequestDto request);
}

