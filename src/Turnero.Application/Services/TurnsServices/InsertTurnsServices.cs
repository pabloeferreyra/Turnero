namespace Turnero.Application.Services.TurnsServices;

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
        catch (TurnSlotConflictException)
        {
            throw;
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

public sealed class TurnSlotConflictException() : Exception("El horario ya fue reservado.")
{
    public const string ConstraintName = "IX_Turns_MedicId_DateTurn_TimeId";
}