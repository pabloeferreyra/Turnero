namespace Turnero.SL.Services.PerinatalBackgroundServices;

public class GetPerinatalBackgroundService(LoggerService logger, IPerinatalBackgroundRepository repository,
    RedisCacheService redisCache,
    IMemoryCache memoryCache) : IGetPerinatalBackgroundService
{
    public async Task<PerinatalBackground> Get(Guid id)
    {
        try
        {
            var cacheKey = $"perinatalBackground:{id}";

            // L1: Check local memory cache
            var cached = memoryCache.Get<PerinatalBackground>(cacheKey);
            if (cached != null) return cached;

            // L2: Check Redis
            var redisCached = await redisCache.GetAsync<PerinatalBackground>(cacheKey);
            if (redisCached != null)
            {
                memoryCache.Set(cacheKey, redisCached);
                return redisCached;
            }

            // Miss: load from database
            var perinatalBackground = await repository.Get(id);
            if (perinatalBackground == null)
            {
                throw new InvalidOperationException($"Perinatal background with ID {id} not found.");
            }

            // Populate both caches
            memoryCache.Set(cacheKey, perinatalBackground);
            await redisCache.SetAsync(cacheKey, perinatalBackground, TimeSpan.FromMinutes(10));

            return perinatalBackground;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(GetPerinatalBackgroundService)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the perinatal background.");
        }
    }
}
public interface IGetPerinatalBackgroundService
{
    Task<PerinatalBackground> Get(Guid id);
}
