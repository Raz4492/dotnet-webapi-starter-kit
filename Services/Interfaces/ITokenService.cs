using SmartAPI.Models;

namespace SmartAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task<bool> ValidateRefreshTokenAsync(string token);
    }
}