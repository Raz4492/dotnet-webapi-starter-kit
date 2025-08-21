using SmartAPI.Data;
using SmartAPI.Models;
using SmartAPI.Models.DTOs;
using SmartAPI.Services.Interfaces;
using Dapper;
using BCrypt.Net;
using SmartAPI.Configuration;

namespace SmartAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IDbContext dbContext, ITokenService tokenService, ILogger<AuthService> logger, JwtSettings jwtSettings )
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _logger = logger;
            _jwtSettings = jwtSettings;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request, string ipAddress)
        {
            var user = await GetUserByUsernameAsync(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) || !user.IsActive)
            {
                _logger.LogWarning("Failed login attempt for username: {Username} from IP: {IpAddress}", 
                    request.Username, ipAddress);
                return null;
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            _logger.LogInformation("User {Username} logged in successfully from IP: {IpAddress}", 
                user.Username, ipAddress);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                }
            };
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
                return null;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = "User"
            };

            using var connection = _dbContext.CreateConnection();
            var sql = @"INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, CreatedAt, IsActive, Role) 
                       OUTPUT INSERTED.Id
                       VALUES (@Username, @Email, @PasswordHash, @FirstName, @LastName, @CreatedAt, @IsActive, @Role)";
            
            var userId = await connection.QuerySingleAsync<int>(sql, user);
            user.Id = userId;

            _logger.LogInformation("New user registered: {Username} with ID: {UserId}", user.Username, userId);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "Registration"
            };

            await _tokenService.SaveRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                }
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            if (!await _tokenService.ValidateRefreshTokenAsync(refreshToken))
            {
                _logger.LogWarning("Invalid refresh token used from IP: {IpAddress}", ipAddress);
                return null;
            }

            var tokenEntity = await _tokenService.GetRefreshTokenAsync(refreshToken);
            if (tokenEntity == null) return null;

            var user = await GetUserByIdAsync(tokenEntity.UserId);
            if (user == null || !user.IsActive) return null;

            await _tokenService.RevokeRefreshTokenAsync(refreshToken);

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _tokenService.SaveRefreshTokenAsync(newRefreshTokenEntity);

            _logger.LogInformation("Token refreshed for user: {Username} from IP: {IpAddress}", 
                user.Username, ipAddress);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                }
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            if (await _tokenService.ValidateRefreshTokenAsync(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken);
                _logger.LogInformation("Token revoked from IP: {IpAddress}", ipAddress);
                return true;
            }
            return false;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) && user.IsActive;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Username = @Username";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }
    }
}