using StackExchange.Redis;

namespace Turnero.SL.Services;

/// <summary>
/// Singleton service managing the Redis ConnectionMultiplexer.
/// One connection per application instance (reuses connections internally).
/// </summary>
public sealed class RedisConnectionService : IDisposable
{
    private readonly ConnectionMultiplexer _connection;

    public RedisConnectionService(string connectionString)
    {
        var config = ConfigurationOptions.Parse(connectionString);
        config.AbortOnConnectFail = false;
        // Preserve defaults from connection string; only fall back if not specified
        if (config.ConnectTimeout <= 0) config.ConnectTimeout = 5000;
        if (config.SyncTimeout <= 0) config.SyncTimeout = 3000;
        _connection = ConnectionMultiplexer.Connect(config);
    }

    public IDatabase GetDatabase() => _connection.GetDatabase();

    public ISubscriber GetSubscriber() => _connection.GetSubscriber();

    public bool IsConnected => _connection.IsConnected;

    public void Dispose() => _connection?.Dispose();
}
