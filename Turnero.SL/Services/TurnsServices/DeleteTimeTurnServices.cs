namespace Turnero.SL.Services.TurnsServices;

public class DeleteTimeTurnServices(LoggerService logger, ITimeTurnRepository timeTurnRepository) : IDeleteTimeTurnServices
{
    private readonly LoggerService _logger = logger;

    public void Delete(TimeTurn timeTurn)
    {
        try
        {
            timeTurnRepository.DeleteTT(timeTurn);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
        }
    }
}

public interface IDeleteTimeTurnServices
{
    void Delete(TimeTurn timeTurn);
}

