namespace Turnero.SL.Services;

public class DeleteTimeTurnServices : IDeleteTimeTurnServices
{
    private readonly ILoggerServices _logger;
    private readonly ITimeTurnRepository _timeTurnRepository;
    public DeleteTimeTurnServices(ILoggerServices logger,
                                  ITimeTurnRepository timeTurnRepository)
    {
        _logger = logger;
        _timeTurnRepository = timeTurnRepository;
    }

    public void Delete(TimeTurn timeTurn)
    {
        try
        {
            _timeTurnRepository.DeleteTT(timeTurn);
            //_logger.Info($"Tiempo {timeTurn.Id} eliminado correctamente");
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
