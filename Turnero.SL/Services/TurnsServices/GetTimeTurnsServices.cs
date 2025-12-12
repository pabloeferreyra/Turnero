namespace Turnero.SL.Services.TurnsServices;

public class GetTimeTurnsServices(LoggerService logger,
                            ITimeTurnRepository timeTurnRepository) : IGetTimeTurnsServices
{
    private readonly LoggerService _logger = logger;
    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public async Task<List<TimeTurn>> GetTimeTurns()
    {
        try
        {
            return await _timeTurnRepository.GetList();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }

    public IQueryable<TimeTurn> GetTimeTurnsQ()
    {
        try
        {
            return _timeTurnRepository.GetQueryable();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return Enumerable.Empty<TimeTurn>().AsQueryable();
        }
    }

    public async Task<TimeTurn> GetTimeTurn(Guid id)
    {
        try
        {
            var result = await _timeTurnRepository.GetbyId(id);
            return result ?? throw new InvalidOperationException("No se encontró el TimeTurn solicitado.");
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            throw;
        }
    }

    public bool TimeTurnViewModelExists(Guid id)
    {
        try
        {
            return _timeTurnRepository.Exists(id);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return false;
        }
    }

    public async Task<List<TimeTurn>> GetCachedTimes()
    {
        try
        {
            var result = await _timeTurnRepository.GetCachedTimes();
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
}

public interface IGetTimeTurnsServices
{
    Task<List<TimeTurn>> GetTimeTurns();
    IQueryable<TimeTurn> GetTimeTurnsQ();
    Task<TimeTurn> GetTimeTurn(Guid id);
    bool TimeTurnViewModelExists(Guid id);
    Task<List<TimeTurn>> GetCachedTimes();
}