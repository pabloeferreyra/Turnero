namespace Turnero.Services;

public class UpdateTurnsServices(ITurnRepository turnRepository) : IUpdateTurnsServices
{
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
        catch (Exception)
        {
        }
    }

    public void Update(Turn turn)
    {
        try
        {
            _turnRepository.UpdateTurn(turn);
        }
        catch (DbUpdateConcurrencyException)
        {
        }
    }

    public void Delete(Turn turn)
    {
        try
        {
            _turnRepository.DeleteTurn(turn);
        }
        catch (DbUpdateConcurrencyException)
        {
        }
    }
}
