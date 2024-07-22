namespace Turnero.Services.Repositories;

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
}
