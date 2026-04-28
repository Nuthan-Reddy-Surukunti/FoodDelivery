using AuthService.Application.Interfaces;
using AuthService.Domain.Enums;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using System.Linq;

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

            var now = DateTime.UtcNow;

            // Reuse an OTP only when it still has enough lifetime left.
            var existingOtp = await _otpTokenRepository.GetByUserIdAsync(userId);
            if (existingOtp != null)
            {
                var remaining = existingOtp.ExpiresAt - now;
                if (remaining > TimeSpan.FromMinutes(2))
                {
                    var remainingMinutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));

                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Your Login OTP",
                        $"Your OTP is: {existingOtp.OTP}. It will expire in about {remainingMinutes} minute(s).");

                    return true;
                }

                // Rotate near-expiry OTP so user receives a fresh code.
                existingOtp.IsUsed = true;
                await _otpTokenRepository.UpdateAsync(existingOtp);
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store new OTP with 10-minute expiration to reduce false-expired reports.
            var otpToken = new OtpToken
            {
                UserId = userId,
                OTP = otp,
                ExpiresAt = now.AddMinutes(10),
                IsUsed = false,
                CreatedAt = now
            };

            await _otpTokenRepository.AddAsync(otpToken);

            // Print OTP to console for development
            Console.WriteLine($"🔐 [OTP] User: {user.Email} | OTP: {otp} | Expires: 10 minutes");

            // Send OTP via email
            await _emailService.SendEmailAsync(
                user.Email,
                "Your Login OTP",
                $"Your OTP is: {otp}. It will expire in 10 minutes. If you requested multiple OTP emails, use the latest OTP.");

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
            var normalizedOtp = new string((otpCode ?? string.Empty).Where(char.IsDigit).ToArray());
            if (normalizedOtp.Length != 6)
                return false;

            var otpToken = await _otpTokenRepository.GetByOtpCodeAsync(userId, normalizedOtp);
            if (otpToken == null)
                return false;

            // Mark OTP as used
            otpToken.IsUsed = true;
            otpToken.VerifiedAt = DateTime.UtcNow;
            await _otpTokenRepository.UpdateAsync(otpToken);

            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Mark user account as verified
            var emailVerified = await _userRepository.SetEmailVerifiedAsync(userId);
            if (!emailVerified)
                return false;

            // RestaurantPartner and Admin transition from Active -> Verified after successful OTP verification.
            if ((user.Role == UserRole.RestaurantPartner || user.Role == UserRole.Admin) && user.AccountStatus == AccountStatus.Active)
            {
                var statusUpdated = await _userRepository.SetAccountStatusAsync(userId, AccountStatus.Verified);
                if (!statusUpdated)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
