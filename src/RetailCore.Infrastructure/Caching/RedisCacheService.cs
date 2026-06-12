using System.Text.Json;
using Microsoft.Extensions.Logging;
using RetailCore.Application.Abstractions;
using StackExchange.Redis;

namespace RetailCore.Infrastructure.Caching;

/// <summary>
/// Redis-backed cache. All operations swallow connection errors and behave as a cache miss
/// so a Redis outage never takes down core POS functionality.
/// </summary>
public class RedisCacheService : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _redis.GetDatabase().StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for {Key}; treating as miss.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await _redis.GetDatabase().StringSetAsync(key, payload, ttl ?? DefaultTtl);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for {Key}; skipping cache write.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis DEL failed for {Key}.", key);
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken ct = default)
    {
        try
        {
            return await _redis.GetDatabase().StringIncrementAsync(key, value);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis INCR failed for {Key}.", key);
            return 0;
        }
    }
}
