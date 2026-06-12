namespace RetailCore.Application.Abstractions;

/// <summary>Distributed cache abstraction (Redis). Implementations must degrade gracefully if the cache is unavailable.</summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Atomically increments a counter and returns the new value (used for request/sales counters).</summary>
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken ct = default);
}
