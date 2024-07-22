namespace Turnero.Services;

public class GetTimeTurnsServices(
                            ITimeTurnRepository timeTurnRepository) : IGetTimeTurnsServices
{
    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public async Task<List<TimeTurn>> GetTimeTurns()
    {
        try
        {
            return await _timeTurnRepository.GetList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public IQueryable<TimeTurn> GetTimeTurnsQ()
    {
        try
        {
            return _timeTurnRepository.GetQueryable();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<TimeTurn> GetTimeTurn(Guid id)
    {
        try
        {
            return await _timeTurnRepository.GetbyId(id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool TimeTurnViewModelExists(Guid id)
    {
        try
        {
            return _timeTurnRepository.Exists(id);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<TimeTurn>> GetCachedTimes()
    {
        try
        {
            return await _timeTurnRepository.GetCachedTimes();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
