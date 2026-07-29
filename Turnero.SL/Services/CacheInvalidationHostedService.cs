using StackExchange.Redis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace Turnero.SL.Services;

/// <summary>
/// Background service that subscribes to Redis Pub/Sub channel "cache:invalidate".
/// When a message is received (a cache key), it clears that key from the local IMemoryCache.
/// This enables cross-instance cache invalidation.
/// </summary>
public class CacheInvalidationHostedService : IHostedService
{
    private readonly RedisConnectionService _redisConnection;
    private readonly IMemoryCache _memoryCache;
    private readonly LoggerService _logger;

    public CacheInvalidationHostedService(
        RedisConnectionService redisConnection,
        IMemoryCache memoryCache,
        LoggerService logger)
    {
        _redisConnection = redisConnection;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = _redisConnection.GetSubscriber();
            subscriber.Subscribe("cache:invalidate", (channel, value) =>
            {
                var cacheKey = (string)value!;
                _memoryCache.Remove(cacheKey);
                _logger.Log($"[CacheInvalidation] Received invalidation for '{cacheKey}' - cleared from local memory cache.");
            });

            _logger.Log("CacheInvalidationHostedService started - subscribed to Redis channel 'cache:invalidate'.");
        }
        catch (Exception ex)
        {
            _logger.Log($"CacheInvalidationHostedService: Redis subscription failed: {ex.Message}. Cross-instance cache invalidation is disabled.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = _redisConnection.GetSubscriber();
            subscriber.Unsubscribe("cache:invalidate");
        }
        catch
        {
            // Ignore unsubscribe errors on shutdown
        }

        return Task.CompletedTask;
    }
}
