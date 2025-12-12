namespace Turnero.SL.Services.PerinatalBackgroundServices;

public class GetPerinatalBackgroundService(LoggerService logger, IPerinatalBackgroundRepository repository) : IGetPerinatalBackgroundService
{
    public async Task<PerinatalBackground> Get(Guid id)
    {
        try
        {
            var perinatalBackground = await repository.Get(id);
            return perinatalBackground ?? throw new InvalidOperationException($"Perinatal background with ID {id} not found.");
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
