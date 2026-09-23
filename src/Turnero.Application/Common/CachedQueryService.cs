namespace Turnero.Application.Common;

/// <summary>
/// Implementación cache-aside genérica sobre IMemoryCache.
/// Antes vivía como método de RepositoryBase; como abstracción propia,
/// Application puede usarla sin conocer detalles de infraestructura.
/// </summary>
public class CachedQueryService(IMemoryCache cache) : ICachedQueryService
{
    private readonly IMemoryCache _cache = cache;

    /// <summary>
    /// Caches data in local memory (IMemoryCache).
    /// On miss, loads from the database and populates the cache.
    /// </summary>
    public async Task<List<TResult>> GetCachedData<TResult>(string cacheKey, Func<Task<List<TResult>>> getDataFunc)
    {
        var data = _cache.Get<List<TResult>>(cacheKey);
        if (data == null)
        {
            data = await getDataFunc();
            _cache.Set(cacheKey, data);
        }
        return data;
    }
}
