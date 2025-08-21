using SmartAPI.Models;
using SmartAPI.Models.DTOs;

namespace SmartAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> ValidateUserAsync(string username, string password);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
    }
}