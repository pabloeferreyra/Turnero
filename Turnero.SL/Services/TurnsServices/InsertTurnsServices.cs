namespace Turnero.SL.Services.TurnsServices;

public class InsertTurnsServices(LoggerService logger, ITurnRepository turnRepository) : IInsertTurnsServices
{
    private readonly LoggerService _logger = logger;
    private readonly ITurnRepository _turnRepository = turnRepository;

    public async Task<bool> CreateTurnAsync(Turn turn)
    {
        try
        {
            await _turnRepository.CreateTurn(turn);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return false;
        }
    }
}

public interface IInsertTurnsServices
{
    Task<bool> CreateTurnAsync(Turn turn);
}