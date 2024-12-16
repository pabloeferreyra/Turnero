namespace Turnero.SL.Services;

public class InsertTurnsServices(ILoggerServices logger, ITurnRepository turnRepository) : IInsertTurnsServices
{
    private readonly ILoggerServices _logger = logger;
    private readonly ITurnRepository _turnRepository = turnRepository;

    public async Task<bool> CreateTurnAsync(Turn turn)
    {
        try
        {
            await _turnRepository.CreateTurn(turn);
            //_ = Task.Run(() =>
            //{
            //    _logger.Debug("Turno agregado correctamente");
            //});
            return true;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return false;
        }
    }
}
