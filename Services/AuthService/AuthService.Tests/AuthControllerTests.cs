using System.Security.Claims;
using AuthService.API.Controllers;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Tests;

[TestFixture]
public class AuthControllerTests
{
    private FakeControllerAuthService _authService = null!;
    private FakeControllerOtpService _otpService = null!;
    private FakeAdminApprovalService _adminApprovalService = null!;
    private FakeRestaurantApprovalService _restaurantApprovalService = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _authService = new FakeControllerAuthService();
        _otpService = new FakeControllerOtpService();
        _adminApprovalService = new FakeAdminApprovalService();
        _restaurantApprovalService = new FakeRestaurantApprovalService();
        _controller = new AuthController(_authService, _otpService, _adminApprovalService, _restaurantApprovalService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Login_WhenServiceSucceedsWithToken_ReturnsOkAndSetsJwtCookie()
    {
        _authService.LoginResult = new AuthRequestDto
        {
            Success = true,
            Token = "jwt-token",
            RefreshToken = "refresh-token"
        };

        var action = await _controller.Login(new LoginRequestDto { Email = "user@example.com", Password = "Password123!" });

        var ok = action as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(_authService.LoginResult));
            Assert.That(_authService.LastLoginRequest!.Email, Is.EqualTo("user@example.com"));
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Does.Contain("jwt=jwt-token"));
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Does.Contain("httponly"));
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Does.Contain("secure"));
        });
    }

    [Test]
    public async Task Login_WhenServiceFails_ReturnsBadRequestAndDoesNotSetCookie()
    {
        _authService.LoginResult = new AuthRequestDto { Success = false, Message = "Invalid credentials." };

        var action = await _controller.Login(new LoginRequestDto { Email = "user@example.com", Password = "bad" });

        var badRequest = action as BadRequestObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value, Is.SameAs(_authService.LoginResult));
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Is.Empty);
        });
    }

    [Test]
    public async Task Register_WhenServiceFails_ReturnsBadRequest()
    {
        _authService.RegisterResult = new AuthRequestDto { Success = false, Message = "Email already registered" };

        var action = await _controller.Register(new RegisterRequestDto { Email = "user@example.com" });

        Assert.That(action, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task VerifyTwoFactor_WhenServiceSucceedsWithToken_ReturnsOkAndSetsJwtCookie()
    {
        _authService.VerifyTwoFactorResult = new AuthRequestDto { Success = true, Token = "two-factor-jwt" };

        var action = await _controller.VerifyTwoFactor(new VerifyTwoFactorOtpRequestDto
        {
            TempToken = "temp",
            Otp = "123456"
        });

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<OkObjectResult>());
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Does.Contain("jwt=two-factor-jwt"));
        });
    }

    [Test]
    public async Task RefreshToken_WhenServiceSucceeds_ReturnsOkWithoutSettingJwtCookie()
    {
        _authService.RefreshTokenResult = new AuthRequestDto { Success = true, Token = "new-jwt" };

        var action = await _controller.RefreshToken(new RefreshTokenDto { RefreshToken = "refresh" });

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<OkObjectResult>());
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Is.Empty);
        });
    }

    [Test]
    public async Task Logout_AlwaysDeletesJwtCookieAndMapsServiceFailureToBadRequest()
    {
        _authService.LogoutResult = new AuthRequestDto { Success = false, Message = "Nope" };

        var action = await _controller.Logout(new RefreshTokenDto { RefreshToken = "refresh" });

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Does.Contain("jwt="));
            Assert.That(_controller.Response.Headers.SetCookie.ToString(), Does.Contain("expires="));
        });
    }

    [Test]
    public async Task ToggleTwoFactor_WithoutNameIdentifierClaim_ReturnsUnauthorizedAndDoesNotCallService()
    {
        var action = await _controller.ToggleTwoFactor(new ToggleTwoFactorRequestDto { Enable = true });

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<UnauthorizedResult>());
            Assert.That(_authService.ToggleTwoFactorCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ChangePassword_WithNameIdentifierClaimCopiesUserIdBeforeCallingService()
    {
        var userId = Guid.NewGuid().ToString();
        SetUser(userId);
        _authService.ChangePasswordResult = new AuthRequestDto { Success = true };
        var dto = new ChangePasswordRequestDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var action = await _controller.ChangePassword(dto);

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<OkObjectResult>());
            Assert.That(dto.UserId, Is.EqualTo(userId));
            Assert.That(_authService.LastChangePasswordRequest, Is.SameAs(dto));
        });
    }

    [Test]
    public async Task CreateAdmin_InvalidEmailReturnsBadRequestBeforeServiceCall()
    {
        var action = await _controller.CreateAdmin(new CreateAdminDto
        {
            FullName = "Admin User",
            Email = "not-an-email",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(_authService.CreateAdminCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ApproveUser_WithAdminClaimCallsApprovalServiceAndReturnsOk()
    {
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetUser(adminId.ToString());
        _adminApprovalService.ApproveUserResult = true;

        var action = await _controller.ApproveUser(new AdminApprovalDto { UserId = userId, Notes = "approved" });

        Assert.Multiple(() =>
        {
            Assert.That(action, Is.TypeOf<OkObjectResult>());
            Assert.That(_adminApprovalService.LastApprovedUserId, Is.EqualTo(userId));
            Assert.That(_adminApprovalService.LastApprovingAdminId, Is.EqualTo(adminId));
            Assert.That(_adminApprovalService.LastApprovalNotes, Is.EqualTo("approved"));
        });
    }

    [Test]
    public async Task ToggleUserStatus_ServiceFailureReturnsBadRequest()
    {
        _authService.ToggleUserStatusResult = new AuthRequestDto { Success = false, Message = "User not found." };

        var action = await _controller.ToggleUserStatus(Guid.NewGuid().ToString());

        Assert.That(action, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetDeliveryAgents_ReturnsServiceResult()
    {
        _authService.DeliveryAgents.Add(new DeliveryAgentDto { UserId = "agent-1", FullName = "Agent One" });

        var action = await _controller.GetDeliveryAgents();

        var ok = action as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(_authService.DeliveryAgents));
        });
    }

    private void SetUser(string userId)
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));
    }

    private sealed class FakeControllerAuthService : IAuthService
    {
        public AuthRequestDto RegisterResult { get; set; } = new() { Success = true };
        public AuthRequestDto LoginResult { get; set; } = new() { Success = true };
        public AuthRequestDto VerifyEmailResult { get; set; } = new() { Success = true };
        public AuthRequestDto GoogleLoginResult { get; set; } = new() { Success = true };
        public AuthRequestDto VerifyTwoFactorResult { get; set; } = new() { Success = true };
        public AuthRequestDto RefreshTokenResult { get; set; } = new() { Success = true };
        public AuthRequestDto LogoutResult { get; set; } = new() { Success = true };
        public AuthRequestDto ForgotPasswordResult { get; set; } = new() { Success = true };
        public AuthRequestDto ResetPasswordResult { get; set; } = new() { Success = true };
        public AuthRequestDto ResetPasswordWithOtpResult { get; set; } = new() { Success = true };
        public AuthRequestDto ToggleTwoFactorResult { get; set; } = new() { Success = true };
        public AuthRequestDto UpdateProfileResult { get; set; } = new() { Success = true };
        public AuthRequestDto ChangePasswordResult { get; set; } = new() { Success = true };
        public AuthRequestDto DeleteUserResult { get; set; } = new() { Success = true };
        public AuthRequestDto CreateAdminResult { get; set; } = new() { Success = true };
        public AuthRequestDto ToggleUserStatusResult { get; set; } = new() { Success = true };
        public AuthRequestDto AdminDeleteUserResult { get; set; } = new() { Success = true };
        public List<DeliveryAgentDto> DeliveryAgents { get; } = new();

        public LoginRequestDto? LastLoginRequest { get; private set; }
        public ChangePasswordRequestDto? LastChangePasswordRequest { get; private set; }
        public int ToggleTwoFactorCalls { get; private set; }
        public int CreateAdminCalls { get; private set; }

        public Task<AuthRequestDto> VerifyEmailOtpAsync(VerifyEmailOtpRequestDto request) => Task.FromResult(VerifyEmailResult);
        public Task<AuthRequestDto> RegisterAsync(RegisterRequestDto request) => Task.FromResult(RegisterResult);
        public Task<AuthRequestDto> LoginAsync(LoginRequestDto request)
        {
            LastLoginRequest = request;
            return Task.FromResult(LoginResult);
        }
        public Task<AuthRequestDto> GoogleLoginAsync(GoogleLoginDto request) => Task.FromResult(GoogleLoginResult);
        public Task<AuthRequestDto> VerifyTwoFactorOtpAsync(VerifyTwoFactorOtpRequestDto request) => Task.FromResult(VerifyTwoFactorResult);
        public Task<AuthRequestDto> RefreshTokenAsync(RefreshTokenDto request) => Task.FromResult(RefreshTokenResult);
        public Task<AuthRequestDto> LogoutAsync(RefreshTokenDto dto) => Task.FromResult(LogoutResult);
        public Task<AuthRequestDto> ForgotPasswordAsync(ForgotPasswordDto request) => Task.FromResult(ForgotPasswordResult);
        public Task<AuthRequestDto> ResetPasswordAsync(ResetPasswordRequestDto request) => Task.FromResult(ResetPasswordResult);
        public Task<AuthRequestDto> ResetPasswordWithOtpAsync(string email, string otp, string newPassword, string confirmPassword) => Task.FromResult(ResetPasswordWithOtpResult);
        public Task<AuthRequestDto> ToggleTwoFactorAsync(string userId, ToggleTwoFactorRequestDto request)
        {
            ToggleTwoFactorCalls++;
            return Task.FromResult(ToggleTwoFactorResult);
        }
        public Task<AuthRequestDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request) => Task.FromResult(UpdateProfileResult);
        public Task<AuthRequestDto> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            LastChangePasswordRequest = request;
            return Task.FromResult(ChangePasswordResult);
        }
        public Task<AuthRequestDto> DeleteUserAsync(DeleteUserRequestDto request) => Task.FromResult(DeleteUserResult);
        public Task<AuthRequestDto> CreateAdminAsync(CreateAdminDto request)
        {
            CreateAdminCalls++;
            return Task.FromResult(CreateAdminResult);
        }
        public Task<List<DeliveryAgentDto>> GetDeliveryAgentsAsync() => Task.FromResult(DeliveryAgents);
        public Task<AuthRequestDto> ToggleUserStatusAsync(string userId) => Task.FromResult(ToggleUserStatusResult);
        public Task<AuthRequestDto> AdminDeleteUserAsync(string userId) => Task.FromResult(AdminDeleteUserResult);
    }

    private sealed class FakeControllerOtpService : IOtpService
    {
        public Task<bool> GenerateAndStoreOtpAsync(Guid userId) => Task.FromResult(true);
        public Task<bool> VerifyOtpAsync(Guid userId, string otpCode) => Task.FromResult(true);
    }

    private sealed class FakeAdminApprovalService : IAdminApprovalService
    {
        public bool ApproveUserResult { get; set; } = true;
        public Guid LastApprovedUserId { get; private set; }
        public Guid LastApprovingAdminId { get; private set; }
        public string? LastApprovalNotes { get; private set; }

        public Task<IEnumerable<dynamic>> GetPendingApprovalsAsync() => Task.FromResult<IEnumerable<dynamic>>(Array.Empty<object>());

        public Task<bool> ApproveUserAsync(Guid userId, Guid approvingAdminId, string? notes = null)
        {
            LastApprovedUserId = userId;
            LastApprovingAdminId = approvingAdminId;
            LastApprovalNotes = notes;
            return Task.FromResult(ApproveUserResult);
        }

        public Task<bool> RejectUserAsync(Guid userId, Guid rejectingAdminId, string reason) => Task.FromResult(true);
    }

    private sealed class FakeRestaurantApprovalService : IRestaurantApprovalService
    {
        public Task<IEnumerable<User>> GetPendingRestaurantsAsync() => Task.FromResult<IEnumerable<User>>(Array.Empty<User>());
        public Task<bool> ApproveRestaurantAsync(Guid restaurantId, Guid adminId, string? notes) => Task.FromResult(true);
        public Task<bool> RejectRestaurantAsync(Guid restaurantId, Guid adminId, string? notes) => Task.FromResult(true);
    }
}
