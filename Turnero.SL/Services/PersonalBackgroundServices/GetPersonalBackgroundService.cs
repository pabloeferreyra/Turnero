namespace Turnero.SL.Services.PersonalBackgroundServices;

public class GetPersonalBackgroundService(LoggerService logger, IPersonalBackgroundRepository repository,
    RedisCacheService redisCache,
    IMemoryCache memoryCache) : IGetPersonalBackgroundService
{
    public async Task<PersonalBackground> GetPersonalBackground(Guid id)
    {
        try
        {
            var cacheKey = $"personalBackground:{id}";

            // L1: Check local memory cache
            var cached = memoryCache.Get<PersonalBackground>(cacheKey);
            if (cached != null) return cached;

            // L2: Check Redis
            var redisCached = await redisCache.GetAsync<PersonalBackground>(cacheKey);
            if (redisCached != null)
            {
                memoryCache.Set(cacheKey, redisCached);
                return redisCached;
            }

            // Miss: load from database
            var personalBackground = await repository.Get(id);
            if (personalBackground == null)
            {
                throw new InvalidOperationException($"Personal background with ID {id} not found.");
            }

            // Populate both caches
            memoryCache.Set(cacheKey, personalBackground);
            await redisCache.SetAsync(cacheKey, personalBackground, TimeSpan.FromMinutes(10));

            return personalBackground;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(GetPersonalBackground)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the personal background.");
        }
    }
}

public interface IGetPersonalBackgroundService
{
    Task<PersonalBackground> GetPersonalBackground(Guid id);
}