namespace Turnero.SL.Services.PersonalBackgroundServices;

public class UpdatePersonalBackgroundService(LoggerService logger, IPersonalBackgroundRepository repository) : IUpdatePersonalBackgroundService
{
    public async Task UpdatePersonalBackground(PersonalBackground data)
    {
        try
        {
            await repository.Update(data);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(UpdatePersonalBackground)}: {ex.Message}");
            throw new Exception("An error occurred while updating the personal background.");
        }
    }
}
public interface IUpdatePersonalBackgroundService
{
    Task UpdatePersonalBackground(PersonalBackground data);
}
