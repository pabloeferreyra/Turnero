namespace Turnero.SL.Services.CongErrorServices;

public class GetCongErrorService(LoggerService logger, ICongErrorsRepository repository,
    RedisCacheService redisCache,
    IMemoryCache memoryCache) : IGetCongErrorService
{
    public async Task<CongErrors> GetCongError(Guid id)
    {
        try
        {
            var cacheKey = $"congErrors:{id}";

            // L1: Check local memory cache
            var cached = memoryCache.Get<CongErrors>(cacheKey);
            if (cached != null) return cached;

            // L2: Check Redis
            var redisCached = await redisCache.GetAsync<CongErrors>(cacheKey);
            if (redisCached != null)
            {
                memoryCache.Set(cacheKey, redisCached);
                return redisCached;
            }

            // Miss: load from database
            var congError = await repository.Get(id);
            if (congError == null)
            {
                throw new InvalidOperationException($"CongError with ID {id} not found.");
            }

            // Populate both caches
            memoryCache.Set(cacheKey, congError);
            await redisCache.SetAsync(cacheKey, congError, TimeSpan.FromMinutes(10));

            return congError;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(GetCongError)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the CongError.");
        }
    }
}

public interface IGetCongErrorService
{
    Task<CongErrors> GetCongError(Guid id);
}
