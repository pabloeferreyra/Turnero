using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Turnero.SL.Services;

/// <summary>
/// Scoped service for Redis cache operations with JSON serialization and Pub/Sub.
/// </summary>
public class RedisCacheService
{
    private readonly IDatabase _db;
    private readonly ISubscriber _subscriber;
    private readonly LoggerService _logger;
    private readonly RedisConnectionService _connectionService;

    /// <summary>
    /// In-memory hit/miss counters per cache key prefix.
    /// </summary>
    public ConcurrentDictionary<string, CacheStats> Stats { get; } = new();

    /// <summary>
    /// Whether the Redis connection was established at the time of the last check.
    /// Note: this value can be stale; use <see cref="CheckConnectionAsync"/> for a
    /// real-time verification.
    /// </summary>
    public bool IsConnected => _connectionService.IsConnected;

    /// <summary>
    /// Actively pings Redis to verify the connection is responsive.
    /// More reliable than <see cref="IsConnected"/> for diagnosing connectivity.
    /// </summary>
    public async Task<bool> CheckConnectionAsync()
    {
        return await _connectionService.IsConnectedAsync();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(RedisConnectionService connectionService, LoggerService logger)
    {
        _connectionService = connectionService;
        _db = connectionService.GetDatabase();
        _subscriber = connectionService.GetSubscriber();
        _logger = logger;
    }

    /// <summary>
    /// Returns the cache key prefix from a full key (e.g., "medics" from "medics").
    /// </summary>
    private static string GetKeyPrefix(string key)
    {
        var colonIndex = key.IndexOf(':');
        return colonIndex > 0 ? key[..colonIndex] : key;
    }

    public class CacheStats
    {
        public long Hits { get; set; }
        public long Misses { get; set; }
    }

    /// <summary>
    /// Gets a cached value by key synchronously. Returns default if not found or on error.
    /// Used when the caller cannot use async (e.g., IQueryable return types).
    /// </summary>
    public virtual T? Get<T>(string key)
    {
        try
        {
            var value = _db.StringGet(key);
            if (!value.HasValue)
            {
                IncrementMiss(key);
                return default;
            }
            IncrementHit(key);
            return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis GET error for key '{key}': {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Sets a cached value synchronously with optional expiration.
    /// Used when the caller cannot use async (e.g., IQueryable return types).
    /// </summary>
    public virtual void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            if (expiry.HasValue)
                _db.StringSet(key, json, expiry.Value);
            else
                _db.StringSet(key, json);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis SET error for key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a key synchronously.
    /// </summary>
    public virtual void Remove(string key)
    {
        try
        {
            _db.KeyDelete(key);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis DELETE error for key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes a message synchronously to a Redis Pub/Sub channel.
    /// </summary>
    public void Publish(string channel, string message)
    {
        try
        {
            _subscriber.Publish(channel, message);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis PUBLISH error on channel '{channel}': {ex.Message}");
        }
    }

    /// <summary>
    /// Subscribes to a Redis Pub/Sub channel with a synchronous handler.
    /// </summary>
    public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
    {
        try
        {
            _subscriber.Subscribe(channel, handler);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis SUBSCRIBE error on channel '{channel}': {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a cached value by key. Returns default if not found or on error.
    /// </summary>
    public virtual async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
            {
                IncrementMiss(key);
                return default;
            }
            IncrementHit(key);
            return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis GET error for key '{key}': {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Sets a cached value with optional expiration.
    /// </summary>
    public virtual async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            if (expiry.HasValue)
                await _db.StringSetAsync(key, json, expiry.Value);
            else
                await _db.StringSetAsync(key, json);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis SET error for key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a key from the cache.
    /// </summary>
    public virtual async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis DELETE error for key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes a message to a Redis Pub/Sub channel.
    /// </summary>
    public virtual async Task PublishAsync(string channel, string message)
    {
        try
        {
            await _subscriber.PublishAsync(channel, message);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis PUBLISH error on channel '{channel}': {ex.Message}");
        }
    }

    /// <summary>
    /// Subscribes to a Redis Pub/Sub channel.
    /// The handler is called for each message received.
    /// </summary>
    public async Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler)
    {
        try
        {
            await _subscriber.SubscribeAsync(channel, handler);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis SUBSCRIBE error on channel '{channel}': {ex.Message}");
        }
    }

    /// <summary>
    /// Unsubscribes from a Redis Pub/Sub channel.
    /// </summary>
    public async Task UnsubscribeAsync(string channel)
    {
        try
        {
            await _subscriber.UnsubscribeAsync(channel);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis UNSUBSCRIBE error on channel '{channel}': {ex.Message}");
        }
    }

    /// <summary>
    /// Returns all cache keys with their TTLs, sizes, and hit/miss stats.
    /// Uses SCAN for safety (non-blocking on large datasets).
    /// </summary>
    public async Task<List<CacheEntryInfo>> GetAllKeysAsync()
    {
        var entries = new List<CacheEntryInfo>();
        try
        {
            long cursor = 0;
            do
            {
                var scanResult = await _db.ExecuteAsync("SCAN", cursor.ToString(), "COUNT", "100");
                var resultArray = (RedisResult[])scanResult!;
                cursor = (long)resultArray[0];
                var keys = (RedisKey[])resultArray[1]!;

                foreach (var key in keys)
                {
                    try
                    {
                        var ttl = await _db.KeyTimeToLiveAsync(key);
                        var value = await _db.StringGetAsync(key);
                        var prefix = GetKeyPrefix(key!);

                        Stats.TryGetValue(prefix, out var stats);

                        entries.Add(new CacheEntryInfo
                        {
                            Key = key!,
                            Prefix = prefix,
                            Ttl = ttl ?? TimeSpan.Zero,
                            SizeBytes = value.HasValue ? value.ToString().Length : 0,
                            Hits = stats?.Hits ?? 0,
                            Misses = stats?.Misses ?? 0
                        });
                    }
                    catch
                    {
                        // Skip keys we can't read
                    }
                }
            } while (cursor != 0);
        }
        catch (Exception ex)
        {
            _logger.Log($"Redis SCAN error: {ex.Message}");
        }
        return entries;
    }

    /// <summary>
    /// Increments the hit counter for a cache key prefix.
    /// </summary>
    private void IncrementHit(string key)
    {
        var prefix = GetKeyPrefix(key);
        Stats.AddOrUpdate(prefix, _ => new CacheStats { Hits = 1, Misses = 0 },
            (_, existing) => { existing.Hits++; return existing; });
    }

    /// <summary>
    /// Increments the miss counter for a cache key prefix.
    /// </summary>
    private void IncrementMiss(string key)
    {
        var prefix = GetKeyPrefix(key);
        Stats.AddOrUpdate(prefix, _ => new CacheStats { Hits = 0, Misses = 1 },
            (_, existing) => { existing.Misses++; return existing; });
    }
}

/// <summary>
/// Represents a cache entry for the admin dashboard.
/// </summary>
public class CacheEntryInfo
{
    public string Key { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public TimeSpan Ttl { get; set; }
    public int SizeBytes { get; set; }
    public long Hits { get; set; }
    public long Misses { get; set; }
}
