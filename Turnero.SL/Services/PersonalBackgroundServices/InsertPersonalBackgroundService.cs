namespace Turnero.SL.Services.PersonalBackgroundServices;

public class InsertPersonalBackgroundService(LoggerService logger, IPersonalBackgroundRepository repository) : IInsertPersonalBackgroundService
{
    public async Task InsertPersonalBackground(PersonalBackground data)
    {
        try
        {
            await repository.Insert(data);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(InsertPersonalBackground)}: {ex.Message}");
            throw new Exception("An error occurred while inserting the personal background.");
        }
    }
}

public interface IInsertPersonalBackgroundService
{
    Task InsertPersonalBackground(PersonalBackground data);
}