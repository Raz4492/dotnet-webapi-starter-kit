using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAPI.Models;
using SmartAPI.Services.Interfaces;

namespace SmartAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IAuthService authService, 
            ICacheService cacheService,
            ILogger<UsersController> logger)
        {
            _authService = authService;
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
        {
            // Try to get from cache first
            var cacheKey = $"user_{id}";
            var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
            
            if (cachedUser != null)
            {
                _logger.LogInformation("User {UserId} retrieved from cache", id);
                return Ok(ApiResponse<User>.SuccessResponse(cachedUser));
            }

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse("User not found"));
            }

            // Cache the user for 5 minutes
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
            _logger.LogInformation("User {UserId} cached for 5 minutes", id);

            return Ok(ApiResponse<User>.SuccessResponse(user));
        }
    }
}