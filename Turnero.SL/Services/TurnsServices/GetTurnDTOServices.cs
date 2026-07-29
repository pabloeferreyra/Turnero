namespace Turnero.SL.Services.TurnsServices;

public class GetTurnDTOServices(ITurnDTORepository turnRepository, RedisCacheService redisCache, IMemoryCache memoryCache) : IGetTurnDTOServices
{
    private readonly string _connectionString = AppSettings.ConnectionString ?? throw new InvalidOperationException("ConnectionString no puede ser nulo.");

    public IQueryable<TurnDTO> GetTurnsDto()
    {
        try
        {
            return turnRepository.GetListDto(_connectionString);
        }
        catch (InvalidOperationException ex)
        {
            throw new ApplicationException("Error al obtener TurnDTOs.", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error inesperado al obtener TurnDTOs.", ex);
        }
    }

    public IQueryable<TurnDTO> GetTurnsDtoByDateAndId(DateOnly date, Guid? id)
    {
        try
        {
            var medicSuffix = id.HasValue ? id.Value.ToString()[..8] : "all";
            var cacheKey = $"turns:{date:yyyy-MM-dd}:{medicSuffix}";

            // L1: Check local memory cache first (ultra-fast)
            var cached = memoryCache.Get<List<TurnDTO>>(cacheKey);
            if (cached != null) return cached.AsQueryable();

            // L2: Check Redis using synchronous API (avoids sync-over-async anti-pattern)
            var redisCached = redisCache.Get<List<TurnDTO>>(cacheKey);
            if (redisCached != null)
            {
                memoryCache.Set(cacheKey, redisCached, TimeSpan.FromSeconds(30));
                return redisCached.AsQueryable();
            }

            // Miss in both caches: fetch from database via stored procedure
            var data = turnRepository.GetListDtoParam(_connectionString, date, id);
            var dataList = data.ToList();

            if (dataList.Count > 0)
            {
                // Populate both caches with short TTL for turn data
                memoryCache.Set(cacheKey, dataList, TimeSpan.FromSeconds(30));
                redisCache.Set(cacheKey, dataList, TimeSpan.FromMinutes(2));
            }

            return dataList.AsQueryable();
        }
        catch (InvalidOperationException ex)
        {
            throw new ApplicationException("Error al obtener TurnDTOs por fecha e ID.", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error inesperado al obtener TurnDTOs por fecha e ID.", ex);
        }
    }
}

public interface IGetTurnDTOServices
{
    IQueryable<TurnDTO> GetTurnsDto();
    IQueryable<TurnDTO> GetTurnsDtoByDateAndId(DateOnly date, Guid? id);
}
