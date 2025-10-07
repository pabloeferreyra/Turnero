namespace Turnero.SL.Services.TurnsServices;

public class UpdateTurnsServices(LoggerService logger, ITurnRepository turnRepository) : IUpdateTurnsServices
{
    private readonly LoggerService _logger = logger;
    private readonly ITurnRepository _turnRepository = turnRepository;

    public void Accessed(Turn turn)
    {
        try
        {
            if (turn.DateTurn.Date <= DateTime.Today.Date)
            {
                _turnRepository.Access(turn);
            }
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
        }
    }

    public void Update(Turn turn)
    {
        try
        {
            _turnRepository.UpdateTurn(turn);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.Log(ex.Message);
        }
    }

    public void Delete(Turn turn)
    {
        try
        {
            _turnRepository.DeleteTurn(turn);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.Log(ex.Message);
        }
    }
}

public interface IUpdateTurnsServices
{
    void Accessed(Turn turn);
    void Update(Turn turn);
    void Delete(Turn turn);
}