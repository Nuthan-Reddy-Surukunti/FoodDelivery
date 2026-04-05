using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOtpService _otpService;
        private readonly IAdminApprovalService _adminApprovalService;

        public AuthController(IAuthService authService, IOtpService otpService, IAdminApprovalService adminApprovalService)
        {
            _authService = authService;
            _otpService = otpService;
            _adminApprovalService = adminApprovalService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Verify email OTP for registration; also supports approved RestaurantPartner first-login OTP by email.
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailOtpRequestDto dto)
        {
            var result = await _authService.VerifyEmailOtpAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorOtpRequestDto dto)
        {
            var result = await _authService.VerifyTwoFactorOtpAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.LogoutAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("toggle-2fa")]
        [Authorize]
        public async Task<IActionResult> ToggleTwoFactor([FromBody] ToggleTwoFactorRequestDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var result = await _authService.ToggleTwoFactorAsync(userId, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequestDto dto)
        {
            var result = await _authService.DeleteUserAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Verify OTP for RestaurantPartner first-login verification
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationDto dto)
        {
            if (dto.UserId == Guid.Empty || string.IsNullOrWhiteSpace(dto.OtpCode))
                return BadRequest(new { Success = false, Message = "userId and otpCode are required." });

            var verified = await _otpService.VerifyOtpAsync(dto.UserId, dto.OtpCode);
            if (!verified)
                return BadRequest(new { Success = false, Message = "Invalid or expired OTP." });

            return Ok(new { Success = true, Message = "OTP verified successfully. Please log in again to receive your access token." });
        }

        /// <summary>
        /// Get all pending RestaurantPartner/Admin approvals (Admin only)
        /// </summary>
        [HttpGet("admin/pending-approvals")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var approvals = await _adminApprovalService.GetPendingApprovalsAsync();
            return Ok(new { Success = true, Data = approvals });
        }

        /// <summary>
        /// Approve a pending RestaurantPartner registration (Admin only)
        /// </summary>
        [HttpPost("admin/approve-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUser([FromBody] AdminApprovalDto dto)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var approved = await _adminApprovalService.ApproveUserAsync(dto.UserId, Guid.Parse(adminId), dto.Notes);
            if (!approved)
                return BadRequest(new { Success = false, Message = "Failed to approve user." });

            return Ok(new { Success = true, Message = "User approved successfully." });
        }

        /// <summary>
        /// Reject a pending RestaurantPartner registration (Admin only)
        /// </summary>
        [HttpPost("admin/reject-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectUser([FromBody] AdminRejectionDto dto)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var rejected = await _adminApprovalService.RejectUserAsync(dto.UserId, Guid.Parse(adminId), dto.Reason);
            if (!rejected)
                return BadRequest(new { Success = false, Message = "Failed to reject user." });

            return Ok(new { Success = true, Message = "User rejected." });
        }

        /// <summary>
        /// Create a new admin account (first admin can be created without auth, subsequent admins require Admin authorization)
        /// </summary>
        [HttpPost("admin/create")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { Success = false, Message = "FullName, Email, and Password are required." });

            if (!dto.Email.Contains("@"))
                return BadRequest(new { Success = false, Message = "Invalid email format." });

            if (dto.Password.Length < 8)
                return BadRequest(new { Success = false, Message = "Password must be at least 8 characters." });

            var result = await _authService.CreateAdminAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
