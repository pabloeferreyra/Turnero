namespace Turnero.SL.Services.PerinatalBackgroundServices;

public class UpdatePerinatalBackgroundService(LoggerService logger, IPerinatalBackgroundRepository repository) : IUpdatePerinatalBackgroundService
{
    public async Task Update(PerinatalBackground data)
    {
        try
        {
            await repository.Update(data);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(UpdatePerinatalBackgroundService)}: {ex.Message}");
            throw new Exception("An error occurred while updating the perinatal background.");
        }
    }
}
public interface IUpdatePerinatalBackgroundService
{
    Task Update(PerinatalBackground data);
}
