namespace Turnero.SL.Services.Repositories;

public interface ITurnRepository
{
    List<Turn> GetList(DateTime? date, Guid? id);
    Task<Turn> GetById(Guid id);
    Task<TurnDTO> GetDTOById(Guid id);
    bool TurnExists(Guid id);
    bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn);
    void Access(Turn turn);
    void DeleteTurn(Turn turn);
    void UpdateTurn(Turn turn);
    Task CreateTurn(Turn turn);
    /// <summary>
    /// Returns turns within a date range with Medic and Time navigation properties included.
    /// Optionally filters by medic ID. Filtering is performed at the database level via EF Core.
    /// </summary>
    List<Turn> GetTurnsByDateRange(DateTime startDate, DateTime endDate, Guid? medicId = null);
}
