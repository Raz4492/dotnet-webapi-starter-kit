namespace SmartAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
    }
}