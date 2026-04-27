using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
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
        private readonly IRestaurantApprovalService _restaurantApprovalService;

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true for HTTPS or cross-origin environments
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("jwt", token, cookieOptions);
        }

        public AuthController(
            IAuthService authService,
            IOtpService otpService,
            IAdminApprovalService adminApprovalService,
            IRestaurantApprovalService restaurantApprovalService)
        {
            _authService = authService;
            _otpService = otpService;
            _adminApprovalService = adminApprovalService;
            _restaurantApprovalService = restaurantApprovalService;
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
            
            if (!string.IsNullOrEmpty(result.Token))
            {
                SetTokenCookie(result.Token);
            }
            
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            
            if (!string.IsNullOrEmpty(result.Token))
            {
                SetTokenCookie(result.Token);
            }
            
            return Ok(result);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorOtpRequestDto dto)
        {
            var result = await _authService.VerifyTwoFactorOtpAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            
            if (!string.IsNullOrEmpty(result.Token))
            {
                SetTokenCookie(result.Token);
            }
            
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
            
            // Clear the cookie regardless of the auth service result
            Response.Cookies.Delete("jwt");
            
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

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var result = await _authService.UpdateProfileAsync(userId, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            dto.UserId = userId;
            var result = await _authService.ChangePasswordAsync(dto);
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

        /// <summary>
        /// Get all delivery agents (for inter-service sync from OrderService).
        /// This endpoint is called by OrderService to sync existing delivery agents.
        /// </summary>
        [HttpGet("admin/delivery-agents")]
        [AllowAnonymous] // Allow OrderService to call this internally
        public async Task<IActionResult> GetDeliveryAgents()
        {
            var agents = await _authService.GetDeliveryAgentsAsync();
            return Ok(agents);
        }

        /// <summary>
        /// Get all pending restaurant approvals (Admin only)
        /// </summary>
        [HttpGet("admin/restaurants/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRestaurants()
        {
            var restaurants = await _restaurantApprovalService.GetPendingRestaurantsAsync();
            return Ok(new { Success = true, Data = restaurants });
        }

        /// <summary>
        /// Approve a pending restaurant registration (Admin only)
        /// </summary>
        [HttpPost("admin/restaurants/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRestaurant([FromBody] AdminApprovalDto dto)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var approved = await _restaurantApprovalService.ApproveRestaurantAsync(dto.UserId, Guid.Parse(adminId), dto.Notes);
            if (!approved)
                return BadRequest(new { Success = false, Message = "Failed to approve restaurant." });

            return Ok(new { Success = true, Message = "Restaurant approved successfully." });
        }

        /// <summary>
        /// Reject a pending restaurant registration (Admin only)
        /// </summary>
        [HttpPost("admin/restaurants/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRestaurant([FromBody] AdminRejectionDto dto)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var rejected = await _restaurantApprovalService.RejectRestaurantAsync(dto.UserId, Guid.Parse(adminId), dto.Reason);
            if (!rejected)
                return BadRequest(new { Success = false, Message = "Failed to reject restaurant." });

            return Ok(new { Success = true, Message = "Restaurant rejected successfully." });
        }
    }
}
