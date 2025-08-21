using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SmartAPI.Configuration;
using SmartAPI.Data;
using SmartAPI.Models;
using SmartAPI.Services.Interfaces;
using Dapper;

namespace SmartAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IDbContext _dbContext;
        private readonly ILogger<TokenService> _logger;

        public TokenService(JwtSettings jwtSettings, IDbContext dbContext, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettings;
            _dbContext = dbContext;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("firstName", user.FirstName),
                new("lastName", user.LastName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM RefreshTokens WHERE Token = @Token AND IsRevoked = 0";
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"INSERT INTO RefreshTokens (Token, UserId, ExpiryDate, IsRevoked, CreatedAt, CreatedByIp) 
                       VALUES (@Token, @UserId, @ExpiryDate, @IsRevoked, @CreatedAt, @CreatedByIp)";
            await connection.ExecuteAsync(sql, refreshToken);
            _logger.LogInformation("Refresh token saved for user {UserId}", refreshToken.UserId);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";
            await connection.ExecuteAsync(sql, new { Token = token });
            _logger.LogInformation("Refresh token revoked: {Token}", token[..10] + "...");
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            return refreshToken != null && !refreshToken.IsRevoked && refreshToken.ExpiryDate > DateTime.UtcNow;
        }
    }
}