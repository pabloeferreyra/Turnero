namespace Turnero.SL.Services.TurnsServices;

public class GetTurnsServices(LoggerService logger,
                        ITurnRepository turnRepository) : IGetTurnsServices
{
    private readonly LoggerService _logger = logger;
    private readonly ITurnRepository _turnRepository = turnRepository;

    public List<Turn> GetTurns(DateTime? dateTurn, Guid? medicId)
    {
        try
        {
            return _turnRepository.GetList(dateTurn, medicId);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }

    public async Task<Turn> GetTurn(Guid id)
    {
        try
        {
            var turn = await _turnRepository.GetById(id);
            return turn ?? new Turn();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return new Turn();
        }
    }

    public async Task<TurnDTO> GetTurnDTO(Guid id)
    {
        try
        {
            var dto = await _turnRepository.GetDTOById(id);
            return dto ?? new TurnDTO();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return new TurnDTO();
        }
    }

    public bool Exists(Guid id)
    {
        try
        {
            return _turnRepository.TurnExists(id);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return false;
        }
    }

    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        try
        {
            return _turnRepository.CheckTurn(medicId, date, timeTurn);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return false;
        }
    }
}

public interface IGetTurnsServices
{
    public List<Turn> GetTurns(DateTime? dateTurn, Guid? medicId);

    public Task<Turn> GetTurn(Guid id);

    public Task<TurnDTO> GetTurnDTO(Guid id);

    public bool Exists(Guid id);

    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn);
}