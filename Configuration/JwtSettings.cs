namespace SmartAPI.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenExpirationInMinutes { get; set; }
        public int RefreshTokenExpirationInDays { get; set; }
    }
}