using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;

namespace AuthService.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IOtpTokenRepository _otpTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public OtpService(IOtpTokenRepository otpTokenRepository, IUserRepository userRepository, IEmailService emailService)
    {
        _otpTokenRepository = otpTokenRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<bool> GenerateAndStoreOtpAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Invalidate previous OTPs
            var existingOtp = await _otpTokenRepository.GetByUserIdAsync(userId);
            if (existingOtp != null && !existingOtp.IsUsed)
            {
                existingOtp.IsUsed = true;
                await _otpTokenRepository.UpdateAsync(existingOtp);
            }

            // Store new OTP with 5-minute expiration
            var otpToken = new OtpToken
            {
                UserId = userId,
                OTP = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otpTokenRepository.AddAsync(otpToken);

            // Send OTP via email
            await _emailService.SendEmailAsync(
                user.Email,
                "Your Login OTP",
                $"Your OTP is: {otp}. It will expire in 5 minutes.");

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> VerifyOtpAsync(Guid userId, string otpCode)
    {
        try
        {
            var otpToken = await _otpTokenRepository.GetByOtpCodeAsync(userId, otpCode);
            if (otpToken == null)
                return false;

            // Mark OTP as used
            otpToken.IsUsed = true;
            otpToken.VerifiedAt = DateTime.UtcNow;
            await _otpTokenRepository.UpdateAsync(otpToken);

            // Mark user account as verified
            await _userRepository.SetEmailVerifiedAsync(userId);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
