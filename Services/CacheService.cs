using StackExchange.Redis;
using System.Text.Json;
using SmartAPI.Services.Interfaces;

namespace SmartAPI.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return null;

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serializedValue, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache key existence: {Key}", key);
                return false;
            }
        }
    }
}