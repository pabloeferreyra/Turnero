namespace Turnero.SL.Services.Interfaces;

public interface IGetTimeTurnsServices
{
    Task<List<TimeTurn>> GetTimeTurns();
    IQueryable<TimeTurn> GetTimeTurnsQ();
    Task<TimeTurn> GetTimeTurn(Guid id);
    bool TimeTurnViewModelExists(Guid id);
    Task<List<TimeTurn>> GetCachedTimes();
}