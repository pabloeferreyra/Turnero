namespace Turnero.SL.Services.Interfaces;

public interface IGetTurnsServices
{
    public List<Turn> GetTurns(DateTime? dateTurn, Guid? medicId);

    public Task<Turn> GetTurn(Guid id);

    public Task<TurnDTO> GetTurnDTO(Guid id);

    public bool Exists(Guid id);

    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn);
}