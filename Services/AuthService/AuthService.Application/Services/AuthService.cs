using System.Security.Cryptography;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly ITwoFactorTokenRepository _twoFactorTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IOtpService _otpService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        ITwoFactorTokenRepository twoFactorTokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService,
        IJwtTokenGenerator jwtTokenGenerator,
        IOtpService otpService)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _twoFactorTokenRepository = twoFactorTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailService = emailService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _otpService = otpService;
    }
    public async Task<AuthRequestDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);
        if(user==null)
        return new AuthRequestDto { Success = true, Message = "If that email exists, a reset link has been sent." };

        var resetToken = GenerateSecureToken();
       await _passwordResetTokenRepository.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            ExpiredAt = DateTime.UtcNow.AddMinutes(30),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        });

        await _emailService.SendEmailAsync(
            dto.Email,
            "Reset Your Password",
            $"Your password reset token is: {resetToken}");

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

        // For RestaurantPartner and Admin roles: generate OTP instead of JWT
        if (user.Role == UserRole.RestaurantPartner || user.Role == UserRole.Admin)
        {
            // Verify email first
            if (!user.IsEmailVerified)
                return new AuthRequestDto { Success = false, Message = "Please verify your email first." };

            // Generate and send OTP
            var otpGenerated = await _otpService.GenerateAndStoreOtpAsync(user.Id);
            if (!otpGenerated)
                return new AuthRequestDto { Success = false, Message = "Failed to generate OTP. Please try again." };

            return new AuthRequestDto()
            {
                Success = true,
                Message = "OTP sent to your email. Please verify to continue.",
                IsTwoFactorRequired = true,
                UserId = user.Id.ToString()
            };
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
            await _emailService.SendEmailAsync(dto.Email,"Two Factor Email",$"Your Otp is {otp}");

            return new AuthRequestDto()
            {
                Success = true,
                Message = "OTP sent to your email.",
                IsTwoFactorRequired = true,
                TempToken = tempToken,
                UserId = user.Id.ToString()
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
            Role = user.Role.ToString()
        };
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
        
        // Determine account status based on role
        var accountStatus = AccountStatus.Verified; // Default for Customer/DeliveryAgent
        if (parsedRole == UserRole.RestaurantPartner || parsedRole == UserRole.Admin)
        {
            // RestaurantPartner and Admin roles require admin approval
            accountStatus = AccountStatus.Pending;
        }
        
        var user = new User()
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Role = parsedRole,
            IsActive = true,
            IsEmailVerified = parsedRole != UserRole.RestaurantPartner && parsedRole != UserRole.Admin, // Email not verified for pending users
            AccountStatus = accountStatus,
            CreatedAt = DateTime.UtcNow
        };
        
        var created = await _userRepository.CreateUserAsync(user,dto.Password);
        if(!created) return new AuthRequestDto { Success = false, Message = "Registration failed." };

        var newUser = await _userRepository.FindByEmailAsync(dto.Email);

        // For Customer/DeliveryAgent: send email verification OTP
        // For RestaurantPartner/Admin: send admin notification (not OTP yet)
        if (parsedRole == UserRole.RestaurantPartner || parsedRole == UserRole.Admin)
        {
            await _emailService.SendEmailAsync(
                "admin@fooddelivery.com", // Send to admin email for approval
                $"New {parsedRole} Registration Pending Approval",
                $"A new {parsedRole} account has been created and is awaiting your approval.\n\n" +
                $"User: {user.FullName}\n" +
                $"Email: {user.Email}\n" +
                $"Role: {parsedRole}\n\n" +
                $"Please review and approve or reject this account.");
            
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
            await _emailService.SendEmailAsync(newUser.Email, "Verify Your Email", $"Your OTP is: {otp}");

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

    public async Task<AuthRequestDto> ToggleTwoFactorAsync(string userId, ToggleTwoFactorRequestDto request)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return new AuthRequestDto { Success = false, Message = "Invalid user ID." };

        var user = await _userRepository.FindByIdAsync(userGuid);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        await _userRepository.SetTwoFactorEnabledAsync(userGuid, request.Enable);

        var message = request.Enable ? "Two factor authentication enabled." : "Two factor authentication disabled.";
        return new AuthRequestDto { Success = true, Message = message };
    }

    public async Task<AuthRequestDto> VerifyEmailOtpAsync(VerifyEmailOtpRequestDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email);
        if (user == null)
            return new AuthRequestDto { Success = false, Message = "User not found." };

        var token = await _emailVerificationTokenRepository.GetLatestByUserIdAsync(user.Id);
        if (token == null || token.IsUsed || token.ExpiredAt < DateTime.UtcNow)
            return new AuthRequestDto { Success = false, Message = "OTP is invalid or expired." };

        if (token.OTP != dto.Otp)
            return new AuthRequestDto { Success = false, Message = "Incorrect OTP." };
        
        await _emailVerificationTokenRepository.MarkUsedAsync(token.Id);
        await _userRepository.SetEmailVerifiedAsync(user.Id);

        return new AuthRequestDto { Success = true, Message = "Email verified successfully." };
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
            Role = user.Role.ToString()
        };
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

        // Send admin creation confirmation email
        await _emailService.SendEmailAsync(
            dto.Email,
            "Admin Account Created",
            $"Your admin account has been successfully created.\n\n" +
            $"Email: {dto.Email}\n\n" +
            $"You can now log in and approve pending RestaurantPartner accounts.");

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
}
