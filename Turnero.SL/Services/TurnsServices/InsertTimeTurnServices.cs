namespace Turnero.SL.Services.TurnsServices;

public class InsertTimeTurnServices(LoggerService logger,
                              ITimeTurnRepository timeTurnRepository) : IInsertTimeTurnServices
{
    private readonly LoggerService _logger = logger;
    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public async Task Create(TimeTurn timeTurnViewModel)
    {
        try
        {
            await _timeTurnRepository.CreateTT(timeTurnViewModel);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
        }
    }
}

public interface IInsertTimeTurnServices 
{ 
    Task Create(TimeTurn timeTurnViewModel); 
}