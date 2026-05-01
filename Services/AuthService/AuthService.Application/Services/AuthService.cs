using System.Security.Cryptography;
using System.Linq;
using QuickBite.Shared.Utilities;

using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MassTransit;
using QuickBite.Shared.Events.Auth;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly ITwoFactorTokenRepository _twoFactorTokenRepository;
    private readonly IOtpTokenRepository _otpTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IOtpService _otpService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        ITwoFactorTokenRepository twoFactorTokenRepository,
        IOtpTokenRepository otpTokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService,
        IJwtTokenGenerator jwtTokenGenerator,
        IOtpService otpService,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _twoFactorTokenRepository = twoFactorTokenRepository;
        _otpTokenRepository = otpTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailService = emailService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _otpService = otpService;
        _publishEndpoint = publishEndpoint;
    }
    public async Task<AuthRequestDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);
        if(user==null)
            return new AuthRequestDto { Success = true, Message = "If that email exists, a reset link has been sent." };

        // Generate OTP for password reset
        var otp = GenerateOtp();
        var now = DateTime.UtcNow;

        // Store OTP for password reset
        var otpToken = new OtpToken
        {
            UserId = user.Id,
            OTP = otp,
            ExpiresAt = now.AddMinutes(10),
            IsUsed = false,
            CreatedAt = now
        };

        await _otpTokenRepository.AddAsync(otpToken);

        // Print OTP to console for development/testing
        // Console.WriteLine($"[PASSWORD RESET OTP] Email: {user.Email} | OTP: {otp} | Expires: 10 minutes");

        // Send OTP via email
        await _emailService.SendEmailAsync(
            dto.Email,
            "Reset Your Password",
            EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

        return new AuthRequestDto { Success = true, Message = "If that email exists, a reset link has been sent." };
    }

    public async Task<AuthRequestDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);

        if (user == null || !user.IsActive)
            return new AuthRequestDto(){Success=false,Message="Invalid credentials."};

        // Check account status - pending approval
        if (user.AccountStatus == AccountStatus.Pending)
            return new AuthRequestDto { Success = false, Message = "Your account is pending admin approval." };
        
        // Check account status - rejected
        if (user.AccountStatus == AccountStatus.Rejected)
            return new AuthRequestDto { Success = false, Message = "Your account has been rejected. Please contact support." };

        var passwordValid = await _userRepository.CheckPasswordAsync(user.Id, dto.Password);
        if (!passwordValid)
            return new AuthRequestDto { Success = false, Message = "Invalid credentials." };

        // For Admin: Check AccountStatus (same as RestaurantPartner)
        if (user.Role == UserRole.Admin)
        {
            // If still pending (shouldn't reach here, but safety check)
            if (user.AccountStatus == AccountStatus.Pending)
                return new AuthRequestDto { Success = false, Message = "Your account is pending admin approval." };

            // If approved but not yet verified by OTP
            if (user.AccountStatus == AccountStatus.Active)
            {
                // Check if 2FA is enabled for this Admin
                if (user.IsTwoFactorVerified)
                {
                    // Generate and send 2FA OTP
                    var otp = GenerateOtp();
                    var tempToken = GenerateSecureToken();
                    var twoFactorToken = new TwoFactorToken()
                    {
                        UserId = user.Id,
                        OTP = otp,
                        TempToken = tempToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _twoFactorTokenRepository.AddAsync(twoFactorToken);
                    
                    // Print OTP to console for development
                    Console.WriteLine($"🔐 [2FA OTP] Email: {dto.Email} | OTP: {otp} | Expires: 5 minutes");
                    
                    await _emailService.SendEmailAsync(dto.Email, "Two Factor Email", EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

                    return new AuthRequestDto()
                    {
                        Success = true,
                        Message = "OTP sent to your email.",
                        IsTwoFactorRequired = true,
                        TempToken = tempToken,
                        UserId = user.Id.ToString(),
                        Role = user.Role.ToString()
                    };
                }

                // 2FA not enabled - send email verification OTP for first-time verification after admin approval
                var otpGenerated = await _otpService.GenerateAndStoreOtpAsync(user.Id);
                if (!otpGenerated)
                    return new AuthRequestDto { Success = false, Message = "Unable to send verification OTP. Please try again." };

                return new AuthRequestDto()
                {
                    Success = true,
                    Message = "If an account exists, a verification OTP has been sent to the registered email.",
                    IsTwoFactorRequired = true,
                    UserId = user.Id.ToString(),
                    Role = user.Role.ToString()
                };
            }

            // If already verified by OTP (AccountStatus = Verified)
            if (user.AccountStatus == AccountStatus.Verified)
            {
                // Check if 2FA is enabled for verified Admin
                if (user.IsTwoFactorVerified)
                {
                    // Generate and send 2FA OTP
                    var otp = GenerateOtp();
                    var tempToken = GenerateSecureToken();
                    var twoFactorToken = new TwoFactorToken()
                    {
                        UserId = user.Id,
                        OTP = otp,
                        TempToken = tempToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _twoFactorTokenRepository.AddAsync(twoFactorToken);
                    await _emailService.SendEmailAsync(dto.Email, "Two Factor Email", EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

                    return new AuthRequestDto()
                    {
                        Success = true,
                        Message = "OTP sent to your email.",
                        IsTwoFactorRequired = true,
                        TempToken = tempToken,
                        UserId = user.Id.ToString(),
                        Role = user.Role.ToString()
                    };
                }

                // 2FA not enabled - proceed with login
                var adminRefreshToken = GenerateSecureToken();
                await _refreshTokenRepository.AddAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = adminRefreshToken,
                    ExpiredAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow
                });

                var adminJwtToken = _jwtTokenGenerator.GenerateToken(user);

                return new AuthRequestDto
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = adminJwtToken,
                    RefreshToken = adminRefreshToken,
                    Role = user.Role.ToString(),
                    UserId = user.Id.ToString(),
                    FullName = user.FullName,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsTwoFactorEnabled = user.IsTwoFactorVerified
                };
            }
        }

        // For RestaurantPartner: Check AccountStatus
        if (user.Role == UserRole.RestaurantPartner)
        {
            // If still pending (shouldn't reach here, but safety check)
            if (user.AccountStatus == AccountStatus.Pending)
                return new AuthRequestDto { Success = false, Message = "Your account is pending admin approval." };

            // If approved but not yet verified by OTP
            if (user.AccountStatus == AccountStatus.Active)
            {
                // Check if 2FA is enabled for this RestaurantPartner
                if (user.IsTwoFactorVerified)
                {
                    // Generate and send 2FA OTP
                    var otp = GenerateOtp();
                    var tempToken = GenerateSecureToken();
                    var twoFactorToken = new TwoFactorToken()
                    {
                        UserId = user.Id,
                        OTP = otp,
                        TempToken = tempToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _twoFactorTokenRepository.AddAsync(twoFactorToken);
                    
                    // Print OTP to console for development
                    Console.WriteLine($"🔐 [2FA OTP] Email: {dto.Email} | OTP: {otp} | Expires: 5 minutes");
                    
                    await _emailService.SendEmailAsync(dto.Email, "Two Factor Email", EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

                    return new AuthRequestDto()
                    {
                        Success = true,
                        Message = "OTP sent to your email.",
                        IsTwoFactorRequired = true,
                        TempToken = tempToken,
                        UserId = user.Id.ToString()
                    };
                }

                // 2FA not enabled - send email verification OTP for first-time verification after admin approval
                var otpGenerated = await _otpService.GenerateAndStoreOtpAsync(user.Id);
                if (!otpGenerated)
                    return new AuthRequestDto { Success = false, Message = "Unable to send verification OTP. Please try again." };

                return new AuthRequestDto()
                {
                    Success = true,
                    Message = "If an account exists, a verification OTP has been sent to the registered email.",
                    IsTwoFactorRequired = true,
                    UserId = user.Id.ToString(),
                    Role = user.Role.ToString()
                };
            }

            // If already verified by OTP (AccountStatus = Verified)
            if (user.AccountStatus == AccountStatus.Verified)
            {
                // Check if 2FA is enabled for verified RestaurantPartner
                if (user.IsTwoFactorVerified)
                {
                    // Generate and send 2FA OTP
                    var otp = GenerateOtp();
                    var tempToken = GenerateSecureToken();
                    var twoFactorToken = new TwoFactorToken()
                    {
                        UserId = user.Id,
                        OTP = otp,
                        TempToken = tempToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _twoFactorTokenRepository.AddAsync(twoFactorToken);
                    await _emailService.SendEmailAsync(dto.Email, "Two Factor Email", EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

                    return new AuthRequestDto()
                    {
                        Success = true,
                        Message = "OTP sent to your email.",
                        IsTwoFactorRequired = true,
                        TempToken = tempToken,
                        UserId = user.Id.ToString()
                    };
                }

                // 2FA not enabled - proceed with login
                var partnerRefreshToken = GenerateSecureToken();
                await _refreshTokenRepository.AddAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = partnerRefreshToken,
                    ExpiredAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow
                });

                var partnerJwtToken = _jwtTokenGenerator.GenerateToken(user);

                return new AuthRequestDto
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = partnerJwtToken,
                    RefreshToken = partnerRefreshToken,
                    Role = user.Role.ToString(),
                    UserId = user.Id.ToString(),
                    FullName = user.FullName,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsTwoFactorEnabled = user.IsTwoFactorVerified
                };
            }
        }

        // For Customer/DeliveryAgent: check email verification
        if (!user.IsEmailVerified)
            return new AuthRequestDto { Success = false, Message = "Please verify your email first." };

        // For Customer/DeliveryAgent with two-factor enabled
        if (user.IsTwoFactorVerified)
        {
            var otp = GenerateOtp();
            var tempToken = GenerateSecureToken();
            var twoFactorToken = new TwoFactorToken()
            {
                UserId = user.Id,
                OTP = otp,
                TempToken = tempToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _twoFactorTokenRepository.AddAsync(twoFactorToken);
            
            // Print OTP to console for development
            Console.WriteLine($"🔐 [2FA OTP] Email: {dto.Email} | OTP: {otp} | Expires: 5 minutes");
            
            await _emailService.SendEmailAsync(dto.Email,"Two Factor Email",EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

            return new AuthRequestDto()
            {
                Success = true,
                Message = "OTP sent to your email.",
                IsTwoFactorRequired = true,
                TempToken = tempToken,
                UserId = user.Id.ToString(),
                Role = user.Role.ToString()
            };
        }

        var refreshToken = GenerateSecureToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });

        var jwtToken = _jwtTokenGenerator.GenerateToken(user);

        return new AuthRequestDto
        {
            Success = true,
            Message = "Login successful.",
            Token = jwtToken,
            RefreshToken = refreshToken,
            Role = user.Role.ToString(),
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            IsTwoFactorEnabled = user.IsTwoFactorVerified
        };
    }

    public async Task<AuthRequestDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
        try
        {
            var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
            var user = await _userRepository.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create a new Customer user
                user = new User()
                {
                    FullName = payload.Name ?? "Google User",
                    Email = payload.Email,
                    MobileNumber = string.Empty,
                    Role = UserRole.Customer,
                    IsActive = true,
                    IsEmailVerified = payload.EmailVerified,
                    AccountStatus = AccountStatus.Verified,
                    CreatedAt = DateTime.UtcNow
                };

                // Generate a secure random password
                string securePassword = GenerateSecurePassword();
                var created = await _userRepository.CreateUserAsync(user, securePassword);
                if (!created) return new AuthRequestDto { Success = false, Message = "Failed to create Google user account." };
                user = await _userRepository.FindByEmailAsync(payload.Email);
            }
            else
            {
                // Account exists, check status
                if (!user.IsActive) return new AuthRequestDto { Success = false, Message = "Your account is deactivated." };
                if (user.AccountStatus == AccountStatus.Pending) return new AuthRequestDto { Success = false, Message = "Your account is pending admin approval." };
                if (user.AccountStatus == AccountStatus.Rejected) return new AuthRequestDto { Success = false, Message = "Your account has been rejected. Please contact support." };
            }

            // At this point we have a valid User
            // If they have 2FA enabled, trigger 2FA flow
            if (user.IsTwoFactorVerified)
            {
                var otp = GenerateOtp();
                var tempToken = GenerateSecureToken();
                var twoFactorToken = new TwoFactorToken()
                {
                    UserId = user!.Id,
                    OTP = otp,
                    TempToken = tempToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _twoFactorTokenRepository.AddAsync(twoFactorToken);
                await _emailService.SendEmailAsync(user.Email, "Two Factor Email", EmailTemplateBuilder.GetOtpEmailTemplate(user.FullName ?? user.Email, otp));

                return new AuthRequestDto()
                {
                    Success = true,
                    Message = "OTP sent to your email.",
                    IsTwoFactorRequired = true,
                    TempToken = tempToken,
                    UserId = user.Id.ToString(),
                    Role = user.Role.ToString()
                };
            }

            // Normal login flow
            var refreshToken = GenerateSecureToken();
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user!.Id,
                Token = refreshToken,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

            var jwtToken = _jwtTokenGenerator.GenerateToken(user);

            return new AuthRequestDto
            {
                Success = true,
                Message = "Login successful.",
                Token = jwtToken,
                RefreshToken = refreshToken,
                Role = user.Role.ToString(),
                UserId = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                IsTwoFactorEnabled = user.IsTwoFactorVerified
            };
        }
        catch (Google.Apis.Auth.InvalidJwtException)
        {
            return new AuthRequestDto { Success = false, Message = "Invalid Google token." };
        }
        catch (Exception ex)
        {
            return new AuthRequestDto { Success = false, Message = "An error occurred during Google login." };
        }
    }

    private string GenerateSecurePassword()
    {
        // Must contain upper, lower, digit, non-alphanumeric, and at least 8 chars
        return Guid.NewGuid().ToString("N").Substring(0, 10) + "A1!a";
    }

    public async Task<AuthRequestDto> LogoutAsync(RefreshTokenDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.RefreshToken))
    {
        return new AuthRequestDto
        {
            Success = true,
            Message = "Logged out successfully."
        };
    }

    var token = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);

    if (token != null && !token.IsRevoked)
    {
        await _refreshTokenRepository.RevokeAsync(dto.RefreshToken);
    }

    return new AuthRequestDto
    {
        Success = true,
        Message = "Logged out successfully."
    };
}

    public async Task<AuthRequestDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);

        if (token == null || token.IsRevoked || token.ExpiredAt < DateTime.UtcNow)
            return new AuthRequestDto { Success = false, Message = "Refresh token is invalid or expired." };

        await _refreshTokenRepository.RevokeAsync(dto.RefreshToken);

        var newRefreshToken = GenerateSecureToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = token.UserId,
            Token = newRefreshToken,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });

        var user = await _userRepository.FindByIdAsync(token.UserId);
        var jwtToken = _jwtTokenGenerator.GenerateToken(user!);

        return new AuthRequestDto
        {
            Success = true,
            Message = "Token refreshed.",
            Token = jwtToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<AuthRequestDto> RegisterAsync(RegisterRequestDto dto)
    {
        var exsistingUser = await _userRepository.FindByEmailAsync(dto.Email);
        if(exsistingUser!=null) return new AuthRequestDto(){Success=false,Message="Email already registered"};
        
        // Parse role: default to Customer if not specified
        if (!Enum.TryParse<UserRole>(dto.Role, out var parsedRole))
            parsedRole = UserRole.Customer;

        // Admin accounts must be created through the dedicated admin creation flow.
        // This prevents public self-registration from producing pending admin accounts.
        if (parsedRole == UserRole.Admin)
        {
            return new AuthRequestDto
            {
                Success = false,
                Message = "Admin accounts cannot be created through public registration. Use the admin creation flow."
            };
        }
        
        // Determine account status based on role
        var accountStatus = AccountStatus.Verified; // Default for Customer/DeliveryAgent
        if (parsedRole == UserRole.RestaurantPartner)
        {
            // RestaurantPartner and Admin roles require admin approval
            accountStatus = AccountStatus.Pending;
        }
        
        var user = new User()
        {
            FullName = dto.FullName,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            Role = parsedRole,
            IsActive = true,
            IsEmailVerified = parsedRole != UserRole.RestaurantPartner, // Email not verified for pending users
            AccountStatus = accountStatus,
            CreatedAt = DateTime.UtcNow
        };
        
        var created = await _userRepository.CreateUserAsync(user,dto.Password);
        if(!created) return new AuthRequestDto { Success = false, Message = "Registration failed." };

        var newUser = await _userRepository.FindByEmailAsync(dto.Email);

        // Publish UserRegisteredEvent to notify other services
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            UserId = newUser!.Id,
            Email = newUser.Email,
            FullName = newUser.FullName,
            Role = parsedRole.ToString(),
            PhoneNumber = newUser.MobileNumber
        });

        // For Customer/DeliveryAgent: send email verification OTP
        // For RestaurantPartner/Admin: send admin notification (not OTP yet)
        if (parsedRole == UserRole.RestaurantPartner || parsedRole == UserRole.Admin)
        {
            await _emailService.SendEmailAsync(
                "surkuntinuthanreddy@gmail.com", // Send to admin email for approval
                $"New {parsedRole} Registration Pending Approval",
                EmailTemplateBuilder.GetGenericNotificationTemplate(
                    $"New {parsedRole} Registration",
                    $"A new {parsedRole} account has been created and is awaiting your approval.\n\n" +
                    $"User: {user.FullName}\n" +
                    $"Email: {user.Email}\n" +
                    $"Role: {parsedRole}\n\n" +
                    $"Please review and approve or reject this account.",
                    "Review Application",
                    "http://localhost:3000/admin/approvals"));
            
            return new AuthRequestDto()
            {
                Success = true,
                Message = $"Registration successful! Your {parsedRole} account is pending admin approval. You will receive an email once approved."
            };
        }
        else
        {
            // Send email verification OTP for Customer/DeliveryAgent
            var otp = GenerateOtp();
            var verificationToken = new EmailVerificationToken()
            {
                UserId = newUser!.Id,
                OTP = otp,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            await _emailVerificationTokenRepository.AddAsync(verificationToken);
            
            // Print OTP to console for development
            Console.WriteLine($"🔐 [REGISTRATION OTP] Email: {newUser.Email} | OTP: {otp} | Expires: 10 minutes");
            Console.Out.Flush();
            
            await _emailService.SendEmailAsync(newUser.Email, "Verify Your Email", EmailTemplateBuilder.GetOtpEmailTemplate(newUser.FullName ?? newUser.Email, otp));

            return new AuthRequestDto(){Success=true,Message="Registration successful. Please verify your email"};
        }
    }

    public async Task<AuthRequestDto> ResetPasswordAsync(ResetPasswordRequestDto dto)
{
    // Step 1: Validate token first (source of truth)
    var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(dto.Token);

    if (resetToken == null || resetToken.IsUsed || resetToken.ExpiredAt < DateTime.UtcNow)
    {
        return new AuthRequestDto
        {
            Success = false,
            Message = "Invalid or expired reset request."
        };
    }

    // Step 2: Validate password match
    if (dto.NewPassword != dto.ConfirmPassword)
    {
        return new AuthRequestDto
        {
            Success = false,
            Message = "Passwords do not match."
        };
    }

    // Step 3: Use UserId from TOKEN (not from DTO)
    var updated = await _userRepository.UpdatePasswordAsync(resetToken.UserId, dto.NewPassword);

    if (!updated)
    {
        return new AuthRequestDto
        {
            Success = false,
            Message = "Unable to reset password."
        };
    }

    // Step 4: Mark token as used
    await _passwordResetTokenRepository.MarkUsedAsync(resetToken.Id);

    return new AuthRequestDto
    {
        Success = true,
        Message = "Password reset successful."
    };
}

    public async Task<AuthRequestDto> ResetPasswordWithOtpAsync(string email, string otp, string newPassword, string confirmPassword)
    {
        // Step 1: Validate password match
        if (newPassword != confirmPassword)
        {
            return new AuthRequestDto
            {
                Success = false,
                Message = "Passwords do not match."
            };
        }

        // Step 2: Find user
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null)
        {
            return new AuthRequestDto
            {
                Success = false,
                Message = "User not found."
            };
        }

        // Step 3: Validate OTP
        var otpToken = await _otpTokenRepository.GetByOtpCodeAsync(user.Id, otp);
        if (otpToken == null || otpToken.IsUsed || otpToken.ExpiresAt < DateTime.UtcNow)
        {
            return new AuthRequestDto
            {
                Success = false,
                Message = "Invalid or expired OTP."
            };
        }

        // Step 4: Update password
        var updated = await _userRepository.UpdatePasswordAsync(user.Id, newPassword);
        if (!updated)
        {
            return new AuthRequestDto
            {
                Success = false,
                Message = "Unable to reset password."
            };
        }

        // Step 5: Mark OTP as used
        otpToken.IsUsed = true;
        otpToken.VerifiedAt = DateTime.UtcNow;
        await _otpTokenRepository.UpdateAsync(otpToken);

        return new AuthRequestDto
        {
            Success = true,
            Message = "Password reset successful."
        };
    }

    public async Task<AuthRequestDto> ToggleTwoFactorAsync(string userId, ToggleTwoFactorRequestDto request)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return new AuthRequestDto { Success = false, Message = "Invalid user ID." };

        var user = await _userRepository.FindByIdAsync(userGuid);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        var updateSucceeded = await _userRepository.SetTwoFactorEnabledAsync(userGuid, request.Enable);
        if (!updateSucceeded)
            return new AuthRequestDto { Success = false, Message = "Failed to update two-factor authentication settings." };

        var message = request.Enable ? "Two factor authentication enabled." : "Two factor authentication disabled.";
        return new AuthRequestDto { Success = true, Message = message };
    }

    public async Task<AuthRequestDto> VerifyEmailOtpAsync(VerifyEmailOtpRequestDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        var normalizedOtp = new string((dto.Otp ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalizedOtp.Length != 6)
            return new AuthRequestDto { Success = false, Message = "OTP is invalid or expired." };

        // Backward-compatible path: approved RestaurantPartner or Admin first-login OTP can be verified by email.
        if ((user.Role == UserRole.RestaurantPartner || user.Role == UserRole.Admin) && user.AccountStatus == AccountStatus.Active)
        {
            var otpVerified = await _otpService.VerifyOtpAsync(user.Id, normalizedOtp);
            if (!otpVerified)
                return new AuthRequestDto { Success = false, Message = "OTP is invalid or expired." };

            // Generate JWT token for approved RestaurantPartner/Admin
            var refreshToken = GenerateSecureToken();
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

            var jwtToken = _jwtTokenGenerator.GenerateToken(user);

            return new AuthRequestDto 
            { 
                Success = true, 
                Message = "Email verified successfully.",
                Token = jwtToken,
                RefreshToken = refreshToken,
                UserId = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Role = user.Role.ToString(),
                IsTwoFactorEnabled = user.IsTwoFactorVerified
            };
        }

        var token = await _emailVerificationTokenRepository.GetLatestByUserIdAsync(user.Id);
        if (token == null || token.IsUsed || token.ExpiredAt < DateTime.UtcNow)
            return new AuthRequestDto { Success = false, Message = "OTP is invalid or expired." };

        if (token.OTP != normalizedOtp)
            return new AuthRequestDto { Success = false, Message = "Incorrect OTP." };
        
        await _emailVerificationTokenRepository.MarkUsedAsync(token.Id);
        await _userRepository.SetEmailVerifiedAsync(user.Id);

        // Generate JWT token for Customer/DeliveryAgent after email verification
        var customerRefreshToken = GenerateSecureToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = customerRefreshToken,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });

        var customerJwtToken = _jwtTokenGenerator.GenerateToken(user);

        return new AuthRequestDto 
        { 
            Success = true, 
            Message = "Email verified successfully.",
            Token = customerJwtToken,
            RefreshToken = customerRefreshToken,
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            Role = user.Role.ToString(),
            IsTwoFactorEnabled = user.IsTwoFactorVerified
        };
    }

    public async Task<AuthRequestDto> VerifyTwoFactorOtpAsync(VerifyTwoFactorOtpRequestDto dto)
    {
        var twoFactorToken = await _twoFactorTokenRepository.GetByTempTokenAsync(dto.TempToken);
        if (twoFactorToken == null || twoFactorToken.IsUsed || twoFactorToken.ExpiresAt < DateTime.UtcNow)
            return new AuthRequestDto { Success = false, Message = "OTP is invalid or expired." };

        if (twoFactorToken.OTP != dto.Otp)
            return new AuthRequestDto { Success = false, Message = "Incorrect OTP." };

        await _twoFactorTokenRepository.MarkUsedAsync(twoFactorToken.Id);

        var user = await _userRepository.FindByIdAsync(twoFactorToken.UserId);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        var refreshToken = GenerateSecureToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });

        var jwtToken = _jwtTokenGenerator.GenerateToken(user);

        return new AuthRequestDto
        {
            Success = true,
            Message = "Login successful.",
            Token = jwtToken,
            RefreshToken = refreshToken,
            Role = user.Role.ToString(),
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            IsTwoFactorEnabled = true
        };
    }

    public async Task<AuthRequestDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return new AuthRequestDto { Success = false, Message = "Invalid user ID." };
        
        var user = await _userRepository.FindByIdAsync(userGuid);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        // Update user profile
        user.FullName = request.FullName;
        user.MobileNumber = request.MobileNumber;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userRepository.UpdateUserAsync(user);
        if (!result)
            return new AuthRequestDto { Success = false, Message = "Failed to update profile." };

        // Publish UserUpdatedEvent to notify other services
        await _publishEndpoint.Publish(new UserUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            UserId = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.MobileNumber
        });

        return new AuthRequestDto
        {
            Success = true,
            Message = "Profile updated successfully.",
            UserId = user.Id.ToString(),
            Email = user.Email,
            FullName = user.FullName,
            MobileNumber = user.MobileNumber,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthRequestDto> ChangePasswordAsync(ChangePasswordRequestDto dto)
    {
        if (!Guid.TryParse(dto.UserId, out var userGuid))
            return new AuthRequestDto { Success = false, Message = "Invalid user ID." };

        var user = await _userRepository.FindByIdAsync(userGuid);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        // Verify current password
        var passwordValid = await _userRepository.CheckPasswordAsync(user.Id, dto.CurrentPassword);
        if (!passwordValid)
            return new AuthRequestDto { Success = false, Message = "Current password is incorrect." };

        // Validate password match
        if (dto.NewPassword != dto.ConfirmPassword)
            return new AuthRequestDto { Success = false, Message = "New passwords do not match." };

        // Validate password length
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            return new AuthRequestDto { Success = false, Message = "Password must be at least 8 characters." };

        // Check if new password is same as current password
        if (dto.CurrentPassword == dto.NewPassword)
            return new AuthRequestDto { Success = false, Message = "New password must be different from current password." };

        // Update password
        var result = await _userRepository.ChangePasswordAsync(user.Id, dto.NewPassword);
        if (!result)
            return new AuthRequestDto { Success = false, Message = "Failed to change password." };

        return new AuthRequestDto { Success = true, Message = "Password changed successfully." };
    }

    public async Task<AuthRequestDto> DeleteUserAsync(DeleteUserRequestDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        var passwordValid = await _userRepository.CheckPasswordAsync(user.Id, dto.Password);
        if (!passwordValid)
            return new AuthRequestDto { Success = false, Message = "Invalid password." };

        var result = await _userRepository.DeleteUserAsync(user.Id);
        if (!result)
            return new AuthRequestDto { Success = false, Message = "Failed to delete user." };

        // Publish UserDeletedEvent to notify other services (like CatalogService to delete restaurants)
        await _publishEndpoint.Publish(new UserDeletedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        });

        return new AuthRequestDto { Success = true, Message = "User account deleted successfully." };
    }

    public async Task<AuthRequestDto> CreateAdminAsync(CreateAdminDto dto)
    {
        // Validate password match
        if (dto.Password != dto.ConfirmPassword)
            return new AuthRequestDto { Success = false, Message = "Passwords do not match." };

        // Check if email already exists
        var existingUser = await _userRepository.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return new AuthRequestDto { Success = false, Message = "Email already registered." };

        // Create admin user with Verified status (fully approved)
        var admin = new User()
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Role = UserRole.Admin,
            IsActive = true,
            IsEmailVerified = true, // Admin email is pre-verified
            AccountStatus = AccountStatus.Verified, // Admin is fully verified
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateUserAsync(admin, dto.Password);
        if (!created)
            return new AuthRequestDto { Success = false, Message = "Failed to create admin account." };

        // Publish UserRegisteredEvent to notify other services
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            UserId = admin.Id,
            Email = admin.Email,
            FullName = admin.FullName,
            Role = admin.Role.ToString()
        });

        // Send admin creation confirmation email
        await _emailService.SendEmailAsync(
            dto.Email,
            "Admin Account Created",
            EmailTemplateBuilder.GetGenericNotificationTemplate(
                "Admin Account Created",
                $"Your admin account has been successfully created.\n\n" +
                $"Email: {dto.Email}\n\n" +
                $"You can now log in and approve pending RestaurantPartner accounts.",
                "Log In Now",
                "http://localhost:3000/login"));

        return new AuthRequestDto
        {
            Success = true,
            Message = "Admin account created successfully."
        };
    }

    private static string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<List<DeliveryAgentDto>> GetDeliveryAgentsAsync()
    {
        var allUsers = await _userRepository.GetAllAsync();
        var agents = allUsers
            .Where(u => u.Role == UserRole.DeliveryAgent && !string.IsNullOrEmpty(u.Email))
            .Select(u => new DeliveryAgentDto
            {
                UserId = u.Id.ToString(),
                FullName = u.FullName ?? "Unknown",
                Email = u.Email,
                PhoneNumber = u.MobileNumber
            })
            .ToList();
        return agents;
    }

    public async Task<AuthRequestDto> ToggleUserStatusAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return new AuthRequestDto { Success = false, Message = "Invalid user ID." };

        var user = await _userRepository.FindByIdAsync(userGuid);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        user.IsActive = !user.IsActive;
        var updated = await _userRepository.UpdateUserAsync(user);

        if (!updated)
            return new AuthRequestDto { Success = false, Message = "Failed to update user status." };

        // Publish event
        await _publishEndpoint.Publish(new UserStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            UserId = user.Id,
            IsActive = user.IsActive,
            Role = user.Role.ToString()
        });

        return new AuthRequestDto 
        { 
            Success = true, 
            Message = user.IsActive ? "User activated successfully." : "User suspended successfully." 
        };
    }

    public async Task<AuthRequestDto> AdminDeleteUserAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return new AuthRequestDto { Success = false, Message = "Invalid user ID." };

        var user = await _userRepository.FindByIdAsync(userGuid);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        // We can use the existing DeleteUserAsync flow, but that one verifies the password in DeleteUserRequestDto.
        // For Admin delete, we bypass password verification.
        var deleted = await _userRepository.DeleteUserAsync(userGuid);

        if (!deleted)
            return new AuthRequestDto { Success = false, Message = "Failed to delete user." };

        // Publish event
        await _publishEndpoint.Publish(new UserDeletedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        });

        return new AuthRequestDto 
        { 
            Success = true, 
            Message = "User deleted successfully." 
        };
    }
}
