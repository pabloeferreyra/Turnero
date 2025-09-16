namespace Turnero.SL.Services;

public class GetTimeTurnsServices(ILoggerServices logger,
                            ITimeTurnRepository timeTurnRepository) : IGetTimeTurnsServices
{
    private readonly ILoggerServices _logger = logger;
    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public async Task<List<TimeTurn>> GetTimeTurns()
    {
        try
        {
            return await _timeTurnRepository.GetList();
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return [];
        }
    }

    public IQueryable<TimeTurn> GetTimeTurnsQ()
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //    _logger.Debug("Tiempos obtenidos");
            //});
            return _timeTurnRepository.GetQueryable();
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return Enumerable.Empty<TimeTurn>().AsQueryable();
        }
    }

    public async Task<TimeTurn> GetTimeTurn(Guid id)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //    _logger.Info($"Tiempo {id} obtenido");
            //});
            var result = await _timeTurnRepository.GetbyId(id);
            return result ?? throw new InvalidOperationException("No se encontró el TimeTurn solicitado.");
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            throw;
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
            //_logger.Error(ex.Message, ex);
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
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return [];
        }
    }
}
