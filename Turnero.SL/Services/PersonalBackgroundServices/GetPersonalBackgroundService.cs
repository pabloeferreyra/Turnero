namespace Turnero.SL.Services.PersonalBackgroundServices;

public class GetPersonalBackgroundService(LoggerService logger, IPersonalBackgroundRepository repository) : IGetPersonalBackgroundService
{
    public async Task<PersonalBackground> GetPersonalBackground(Guid id)
    {
        try
        {
            var personalBackground = await repository.Get(id);
            if (personalBackground == null)
            {
                throw new InvalidOperationException($"Personal background with ID {id} not found.");
            }
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