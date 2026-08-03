namespace Turnero.SL.Services.PerinatalBackgroundServices;

public class GetPerinatalBackgroundService(LoggerService logger, IPerinatalBackgroundRepository repository,
    IMemoryCache memoryCache) : IGetPerinatalBackgroundService
{
    public async Task<PerinatalBackground> Get(Guid id)
    {
        try
        {
            var cacheKey = $"perinatalBackground:{id}";

            // Check local memory cache
            var cached = memoryCache.Get<PerinatalBackground>(cacheKey);
            if (cached != null) return cached;

            // Miss: load from database
            var perinatalBackground = await repository.Get(id) ?? throw new InvalidOperationException($"Perinatal background with ID {id} not found.");

            // Populate memory cache
            memoryCache.Set(cacheKey, perinatalBackground);

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
