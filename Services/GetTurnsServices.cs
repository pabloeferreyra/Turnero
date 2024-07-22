namespace Turnero.Services;

public class GetTurnsServices(
                        ITurnRepository turnRepository) : IGetTurnsServices
{
    private readonly ITurnRepository _turnRepository = turnRepository;

    public List<Turn> GetTurns(DateTime? dateTurn, Guid? medicId)
    {
        try
        {
            return _turnRepository.GetList(dateTurn, medicId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Turn> GetTurn(Guid id)
    {
        try
        {
            return await _turnRepository.GetById(id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<TurnDTO> GetTurnDTO(Guid id)
    {
        try
        {
            return await _turnRepository.GetDTOById(id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool Exists(Guid id)
    {
        try
        {
            return _turnRepository.TurnExists(id);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        try
        {
            return _turnRepository.CheckTurn(medicId, date, timeTurn);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
