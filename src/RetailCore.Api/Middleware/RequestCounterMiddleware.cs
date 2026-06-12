using RetailCore.Application.Abstractions;

namespace RetailCore.Api.Middleware;

/// <summary>Increments a global Redis request counter for observability.</summary>
public class RequestCounterMiddleware
{
    private readonly RequestDelegate _next;

    public RequestCounterMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ICacheService cache)
    {
        await cache.IncrementAsync(CacheKeys.RequestCounter);
        await _next(context);
    }
}
