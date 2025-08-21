using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAPI.Models;
using SmartAPI.Models.DTOs;
using SmartAPI.Services.Interfaces;

namespace SmartAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Invalid request data"));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(request, ipAddress);

            if (result == null)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Invalid username or password"));
            }

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Invalid request data"));
            }

            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Username already exists"));
            }

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful"));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Refresh token is required"));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            if (result == null)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Invalid or expired refresh token"));
            }

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully"));
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token is required"));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

            if (!result)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid refresh token"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, "Token revoked successfully"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(ApiResponse<UserInfo>.ErrorResponse("Invalid user token"));
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<UserInfo>.ErrorResponse("User not found"));
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };

            return Ok(ApiResponse<UserInfo>.SuccessResponse(userInfo));
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"]!;

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}