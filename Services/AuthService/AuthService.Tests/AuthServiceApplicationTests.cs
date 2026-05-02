using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Tests.TestDoubles;
using QuickBite.Shared.Events.Auth;

namespace AuthService.Tests;

[TestFixture]
public class AuthServiceApplicationTests
{
    private AuthServiceTestHarness _harness = null!;
    private Application.Services.AuthService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _harness = new AuthServiceTestHarness();
        _service = _harness.CreateService();
    }

    [Test]
    public async Task RegisterAsync_CustomerCreatesUserPublishesEventAndSendsVerificationOtp()
    {
        var result = await _service.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Nuthan Reddy",
            Email = "nuthan@example.com",
            MobileNumber = "9999999999",
            Password = "Password123!",
            Role = "Customer"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Registration successful. Please verify your email"));
            Assert.That(_harness.Users.Users.Single().Role, Is.EqualTo(UserRole.Customer));
            Assert.That(_harness.Users.Users.Single().AccountStatus, Is.EqualTo(AccountStatus.Verified));
            Assert.That(_harness.Users.Users.Single().IsEmailVerified, Is.True);
            Assert.That(_harness.EmailVerificationTokens.Tokens, Has.Count.EqualTo(1));
            Assert.That(_harness.Email.SentEmails.Single().ToEmail, Is.EqualTo("nuthan@example.com"));
            Assert.That(_harness.Email.SentEmails.Single().Subject, Is.EqualTo("Verify Your Email"));
        });

        var published = _harness.Publisher.PublishedMessages.OfType<UserRegisteredEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(published.Email, Is.EqualTo("nuthan@example.com"));
            Assert.That(published.FullName, Is.EqualTo("Nuthan Reddy"));
            Assert.That(published.Role, Is.EqualTo(UserRole.Customer.ToString()));
            Assert.That(published.PhoneNumber, Is.EqualTo("9999999999"));
        });
    }

    [Test]
    public async Task RegisterAsync_RejectsPublicAdminRegistration()
    {
        var result = await _service.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            Role = "Admin"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("Admin accounts cannot be created"));
            Assert.That(_harness.Users.Users, Is.Empty);
            Assert.That(_harness.Publisher.PublishedMessages, Is.Empty);
        });
    }

    [Test]
    public async Task RegisterAsync_RestaurantPartnerIsPendingAndNotifiesAdmin()
    {
        var result = await _service.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Partner One",
            Email = "partner@example.com",
            MobileNumber = "8888888888",
            Password = "Password123!",
            Role = "RestaurantPartner"
        });

        var user = _harness.Users.Users.Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Does.Contain("pending admin approval"));
            Assert.That(user.Role, Is.EqualTo(UserRole.RestaurantPartner));
            Assert.That(user.AccountStatus, Is.EqualTo(AccountStatus.Pending));
            Assert.That(user.IsEmailVerified, Is.False);
            Assert.That(_harness.EmailVerificationTokens.Tokens, Is.Empty);
            Assert.That(_harness.Email.SentEmails.Single().Subject, Does.Contain("Pending Approval"));
            Assert.That(_harness.Publisher.PublishedMessages.OfType<UserRegisteredEvent>().Single().Role, Is.EqualTo("RestaurantPartner"));
        });
    }

    [Test]
    public async Task LoginAsync_VerifiedCustomerCreatesRefreshTokenAndReturnsJwt()
    {
        var user = CreateUser(role: UserRole.Customer, isEmailVerified: true);
        _harness.Users.Add(user, "Password123!");
        _harness.Jwt.TokenToReturn = "customer-jwt";

        var result = await _service.LoginAsync(new LoginRequestDto
        {
            Email = user.Email,
            Password = "Password123!"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Login successful."));
            Assert.That(result.Token, Is.EqualTo("customer-jwt"));
            Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Role, Is.EqualTo(UserRole.Customer.ToString()));
            Assert.That(_harness.RefreshTokens.AddedTokens.Single().UserId, Is.EqualTo(user.Id));
            Assert.That(_harness.Jwt.Users.Single(), Is.SameAs(user));
        });
    }

    [Test]
    public async Task LoginAsync_UnverifiedCustomerFailsBeforePasswordTokenWork()
    {
        var user = CreateUser(role: UserRole.Customer, isEmailVerified: false);
        _harness.Users.Add(user, "Password123!");

        var result = await _service.LoginAsync(new LoginRequestDto
        {
            Email = user.Email,
            Password = "Password123!"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Please verify your email first."));
            Assert.That(_harness.RefreshTokens.AddedTokens, Is.Empty);
            Assert.That(_harness.Jwt.Users, Is.Empty);
        });
    }

    [Test]
    public async Task LoginAsync_TwoFactorEnabledUserStoresOtpAndReturnsTempToken()
    {
        var user = CreateUser(role: UserRole.Customer, isEmailVerified: true, isTwoFactorEnabled: true);
        _harness.Users.Add(user, "Password123!");

        var result = await _service.LoginAsync(new LoginRequestDto
        {
            Email = user.Email,
            Password = "Password123!"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.IsTwoFactorRequired, Is.True);
            Assert.That(result.TempToken, Is.Not.Null.And.Not.Empty);
            Assert.That(_harness.TwoFactorTokens.Tokens.Single().UserId, Is.EqualTo(user.Id));
            Assert.That(_harness.TwoFactorTokens.Tokens.Single().OTP, Has.Length.EqualTo(6));
            Assert.That(_harness.Email.SentEmails.Single().Subject, Is.EqualTo("Two Factor Email"));
            Assert.That(_harness.RefreshTokens.AddedTokens, Is.Empty);
        });
    }

    [Test]
    public async Task VerifyEmailOtpAsync_ValidCustomerOtpMarksEmailVerifiedAndReturnsTokens()
    {
        var user = CreateUser(role: UserRole.Customer, isEmailVerified: false);
        _harness.Users.Add(user);
        var token = new EmailVerificationToken
        {
            UserId = user.Id,
            OTP = "123456",
            ExpiredAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _harness.EmailVerificationTokens.AddAsync(token);
        _harness.Jwt.TokenToReturn = "verified-jwt";

        var result = await _service.VerifyEmailOtpAsync(new VerifyEmailOtpRequestDto
        {
            Email = user.Email,
            Otp = "123-456"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Token, Is.EqualTo("verified-jwt"));
            Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
            Assert.That(user.IsEmailVerified, Is.True);
            Assert.That(_harness.EmailVerificationTokens.MarkedUsedIds.Single(), Is.EqualTo(token.Id));
            Assert.That(_harness.RefreshTokens.AddedTokens.Single().UserId, Is.EqualTo(user.Id));
        });
    }

    [Test]
    public async Task VerifyTwoFactorOtpAsync_ValidOtpMarksUsedAndReturnsAuthTokens()
    {
        var user = CreateUser(role: UserRole.Admin, accountStatus: AccountStatus.Verified, isTwoFactorEnabled: true);
        _harness.Users.Add(user);
        var token = new TwoFactorToken
        {
            UserId = user.Id,
            OTP = "654321",
            TempToken = "temp-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _harness.TwoFactorTokens.AddAsync(token);
        _harness.Jwt.TokenToReturn = "admin-jwt";

        var result = await _service.VerifyTwoFactorOtpAsync(new VerifyTwoFactorOtpRequestDto
        {
            TempToken = "temp-token",
            Otp = "654321"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Token, Is.EqualTo("admin-jwt"));
            Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
            Assert.That(result.IsTwoFactorEnabled, Is.True);
            Assert.That(_harness.TwoFactorTokens.MarkedUsedIds.Single(), Is.EqualTo(token.Id));
            Assert.That(_harness.RefreshTokens.AddedTokens.Single().UserId, Is.EqualTo(user.Id));
        });
    }

    [Test]
    public async Task RefreshTokenAsync_ValidTokenRevokesOldTokenAndCreatesNewPair()
    {
        var user = CreateUser();
        _harness.Users.Add(user);
        _harness.RefreshTokens.AddExisting(new RefreshToken
        {
            UserId = user.Id,
            Token = "old-refresh",
            ExpiredAt = DateTime.UtcNow.AddMinutes(10)
        });
        _harness.Jwt.TokenToReturn = "fresh-jwt";

        var result = await _service.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = "old-refresh" });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Token, Is.EqualTo("fresh-jwt"));
            Assert.That(result.RefreshToken, Is.Not.Null.And.Not.EqualTo("old-refresh"));
            Assert.That(_harness.RefreshTokens.RevokedTokens.Single(), Is.EqualTo("old-refresh"));
            Assert.That(_harness.RefreshTokens.AddedTokens.Single().UserId, Is.EqualTo(user.Id));
        });
    }

    [Test]
    public async Task LogoutAsync_BlankRefreshTokenStillSucceedsWithoutRepositoryWork()
    {
        var result = await _service.LogoutAsync(new RefreshTokenDto { RefreshToken = " " });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Logged out successfully."));
            Assert.That(_harness.RefreshTokens.RevokedTokens, Is.Empty);
        });
    }

    [Test]
    public async Task ForgotPasswordAsync_ExistingUserStoresOtpAndEmailsGenericResponse()
    {
        var user = CreateUser();
        _harness.Users.Add(user);

        var result = await _service.ForgotPasswordAsync(new ForgotPasswordDto { Email = user.Email });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("If that email exists, a reset link has been sent."));
            Assert.That(_harness.OtpTokens.Tokens.Single().UserId, Is.EqualTo(user.Id));
            Assert.That(_harness.OtpTokens.Tokens.Single().OTP, Has.Length.EqualTo(6));
            Assert.That(_harness.Email.SentEmails.Single().Subject, Is.EqualTo("Reset Your Password"));
        });
    }

    [Test]
    public async Task ResetPasswordAsync_ValidTokenUpdatesPasswordAndMarksTokenUsed()
    {
        var user = CreateUser();
        _harness.Users.Add(user, "OldPassword123!");
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = "reset-token",
            ExpiredAt = DateTime.UtcNow.AddMinutes(5)
        };
        _harness.PasswordResetTokens.AddExisting(resetToken);

        var result = await _service.ResetPasswordAsync(new ResetPasswordRequestDto
        {
            Token = "reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(_harness.PasswordResetTokens.MarkedUsedIds.Single(), Is.EqualTo(resetToken.Id));
        });

        var login = await _service.LoginAsync(new LoginRequestDto { Email = user.Email, Password = "NewPassword123!" });
        Assert.That(login.Success, Is.True);
    }

    [Test]
    public async Task ResetPasswordWithOtpAsync_ValidOtpUpdatesPasswordAndMarksOtpUsed()
    {
        var user = CreateUser();
        _harness.Users.Add(user, "OldPassword123!");
        var otpToken = new OtpToken
        {
            UserId = user.Id,
            OTP = "111222",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _harness.OtpTokens.AddAsync(otpToken);

        var result = await _service.ResetPasswordWithOtpAsync(user.Email, "111222", "NewPassword123!", "NewPassword123!");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(otpToken.IsUsed, Is.True);
            Assert.That(otpToken.VerifiedAt, Is.Not.Null);
        });
    }

    [Test]
    public async Task ToggleTwoFactorAsync_ValidUserUpdatesSetting()
    {
        var user = CreateUser(isTwoFactorEnabled: false);
        _harness.Users.Add(user);

        var result = await _service.ToggleTwoFactorAsync(user.Id.ToString(), new ToggleTwoFactorRequestDto { Enable = true });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Two factor authentication enabled."));
            Assert.That(user.IsTwoFactorVerified, Is.True);
        });
    }

    [Test]
    public async Task UpdateProfileAsync_UpdatesUserAndPublishesEvent()
    {
        var user = CreateUser(fullName: "Old Name", mobile: "111");
        _harness.Users.Add(user);

        var result = await _service.UpdateProfileAsync(user.Id.ToString(), new UpdateProfileRequestDto
        {
            FullName = "New Name",
            MobileNumber = "222"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(user.FullName, Is.EqualTo("New Name"));
            Assert.That(user.MobileNumber, Is.EqualTo("222"));
            Assert.That(result.FullName, Is.EqualTo("New Name"));
        });

        var published = _harness.Publisher.PublishedMessages.OfType<UserUpdatedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(published.UserId, Is.EqualTo(user.Id));
            Assert.That(published.FullName, Is.EqualTo("New Name"));
            Assert.That(published.PhoneNumber, Is.EqualTo("222"));
        });
    }

    [Test]
    public async Task ChangePasswordAsync_ValidRequestChangesPassword()
    {
        var user = CreateUser();
        _harness.Users.Add(user, "OldPassword123!");

        var result = await _service.ChangePasswordAsync(new ChangePasswordRequestDto
        {
            UserId = user.Id.ToString(),
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        });

        Assert.That(result.Success, Is.True);
        var login = await _service.LoginAsync(new LoginRequestDto { Email = user.Email, Password = "NewPassword123!" });
        Assert.That(login.Success, Is.True);
    }

    [Test]
    public async Task DeleteUserAsync_ValidPasswordDeletesUserAndPublishesEvent()
    {
        var user = CreateUser(role: UserRole.RestaurantPartner);
        _harness.Users.Add(user, "Password123!");

        var result = await _service.DeleteUserAsync(new DeleteUserRequestDto
        {
            Email = user.Email,
            Password = "Password123!"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(_harness.Users.Users, Is.Empty);
        });

        var published = _harness.Publisher.PublishedMessages.OfType<UserDeletedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(published.UserId, Is.EqualTo(user.Id));
            Assert.That(published.Email, Is.EqualTo(user.Email));
            Assert.That(published.Role, Is.EqualTo(UserRole.RestaurantPartner.ToString()));
        });
    }

    [Test]
    public async Task CreateAdminAsync_CreatesVerifiedAdminPublishesEventAndEmailsAdmin()
    {
        var result = await _service.CreateAdminAsync(new CreateAdminDto
        {
            FullName = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var admin = _harness.Users.Users.Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(admin.Role, Is.EqualTo(UserRole.Admin));
            Assert.That(admin.IsEmailVerified, Is.True);
            Assert.That(admin.AccountStatus, Is.EqualTo(AccountStatus.Verified));
            Assert.That(_harness.Email.SentEmails.Single().Subject, Is.EqualTo("Admin Account Created"));
            Assert.That(_harness.Publisher.PublishedMessages.OfType<UserRegisteredEvent>().Single().Role, Is.EqualTo("Admin"));
        });
    }

    [Test]
    public async Task GetDeliveryAgentsAsync_ReturnsOnlyDeliveryAgents()
    {
        var deliveryAgent = CreateUser(role: UserRole.DeliveryAgent, fullName: "Agent One", mobile: "123");
        _harness.Users.Add(deliveryAgent);
        _harness.Users.Add(CreateUser(role: UserRole.Customer, email: "customer@example.com"));

        var agents = await _service.GetDeliveryAgentsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(agents, Has.Count.EqualTo(1));
            Assert.That(agents.Single().UserId, Is.EqualTo(deliveryAgent.Id.ToString()));
            Assert.That(agents.Single().FullName, Is.EqualTo("Agent One"));
            Assert.That(agents.Single().PhoneNumber, Is.EqualTo("123"));
        });
    }

    [Test]
    public async Task ToggleUserStatusAsync_FlipsActiveFlagAndPublishesEvent()
    {
        var user = CreateUser(role: UserRole.Customer, isActive: true);
        _harness.Users.Add(user);

        var result = await _service.ToggleUserStatusAsync(user.Id.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("User suspended successfully."));
            Assert.That(user.IsActive, Is.False);
        });

        var published = _harness.Publisher.PublishedMessages.OfType<UserStatusChangedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(published.UserId, Is.EqualTo(user.Id));
            Assert.That(published.IsActive, Is.False);
            Assert.That(published.Role, Is.EqualTo("Customer"));
        });
    }

    [Test]
    public async Task AdminDeleteUserAsync_DeletesWithoutPasswordAndPublishesEvent()
    {
        var user = CreateUser(role: UserRole.DeliveryAgent);
        _harness.Users.Add(user);

        var result = await _service.AdminDeleteUserAsync(user.Id.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(_harness.Users.Users, Is.Empty);
            Assert.That(_harness.Publisher.PublishedMessages.OfType<UserDeletedEvent>().Single().Role, Is.EqualTo("DeliveryAgent"));
        });
    }

    private static User CreateUser(
        UserRole role = UserRole.Customer,
        AccountStatus accountStatus = AccountStatus.Verified,
        bool isActive = true,
        bool isEmailVerified = true,
        bool isTwoFactorEnabled = false,
        string fullName = "Test User",
        string email = "user@example.com",
        string mobile = "9999999999")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            MobileNumber = mobile,
            Role = role,
            IsActive = isActive,
            IsEmailVerified = isEmailVerified,
            IsTwoFactorVerified = isTwoFactorEnabled,
            AccountStatus = accountStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
