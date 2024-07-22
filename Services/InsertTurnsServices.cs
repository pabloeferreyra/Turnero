namespace Turnero.Services;

public class InsertTurnsServices(ITurnRepository turnRepository) : IInsertTurnsServices
{
    private readonly ITurnRepository _turnRepository = turnRepository;

    public async Task<bool> CreateTurnAsync(Turn turn)
    {
        try
        {
            await _turnRepository.CreateTurn(turn);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
