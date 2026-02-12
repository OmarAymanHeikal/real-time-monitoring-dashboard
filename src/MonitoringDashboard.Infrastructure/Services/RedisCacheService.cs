using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;
using StackExchange.Redis;

namespace MonitoringDashboard.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer? _redis;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger, IConnectionMultiplexer? redis = null)
    {
        _cache = cache;
        _logger = logger;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached value for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            
            _logger.LogInformation("Cached value for key: {Key} with expiration: {Expiration}", key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists: {Key}", key);
            return false;
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis == null)
            {
                _logger.LogWarning("Redis connection not available for pattern removal. Prefix: {Prefix}", prefix);
                return;
            }

            var db = _redis.GetDatabase();
            var endpoint = _redis.GetEndPoints().FirstOrDefault();
            
            if (endpoint == null)
            {
                _logger.LogWarning("No Redis endpoint found for pattern removal");
                return;
            }

            var server = _redis.GetServer(endpoint);
            var pattern = $"{prefix}*";
            var keys = server.Keys(pattern: pattern).ToArray();

            if (keys.Length > 0)
            {
                await db.KeyDeleteAsync(keys);
                _logger.LogInformation("Removed {Count} keys matching prefix: {Prefix}", keys.Length, prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing keys by prefix: {Prefix}", prefix);
        }
    }
}
