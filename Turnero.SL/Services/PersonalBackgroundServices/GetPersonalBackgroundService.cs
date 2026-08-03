namespace Turnero.SL.Services.PersonalBackgroundServices;

public class GetPersonalBackgroundService(LoggerService logger, IPersonalBackgroundRepository repository,
    IMemoryCache memoryCache) : IGetPersonalBackgroundService
{
    public async Task<PersonalBackground> GetPersonalBackground(Guid id)
    {
        try
        {
            var cacheKey = $"personalBackground:{id}";

            // Check local memory cache
            var cached = memoryCache.Get<PersonalBackground>(cacheKey);
            if (cached != null) return cached;

            // Miss: load from database
            var personalBackground = await repository.Get(id) ?? throw new InvalidOperationException($"Personal background with ID {id} not found.");

            // Populate memory cache
            memoryCache.Set(cacheKey, personalBackground);

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