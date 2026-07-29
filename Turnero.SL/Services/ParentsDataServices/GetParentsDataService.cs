namespace Turnero.SL.Services.ParentsDataServices;

public class GetParentsDataService(IParentsDataRepository parentsDataRepository,
    RedisCacheService redisCache,
    IMemoryCache memoryCache) : IGetParentsDataService
{
    public async Task<ParentsData?> GetParentsData(Guid id)
    {
        var cacheKey = $"parentsData:{id}";

        // L1: Check local memory cache
        var cached = memoryCache.Get<ParentsData>(cacheKey);
        if (cached != null) return cached;

        // L2: Check Redis
        var redisCached = await redisCache.GetAsync<ParentsData>(cacheKey);
        if (redisCached != null)
        {
            memoryCache.Set(cacheKey, redisCached);
            return redisCached;
        }

        // Miss: load from database
        var data = await parentsDataRepository.Get(id);

        if (data != null)
        {
            memoryCache.Set(cacheKey, data);
            await redisCache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
        }

        return data;
    }
}
public interface IGetParentsDataService
{
    Task<ParentsData?> GetParentsData(Guid id);
}
