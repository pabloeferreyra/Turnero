namespace Turnero.SL.Services;

public class InsertTimeTurnServices(ILoggerServices logger,
                              ITimeTurnRepository timeTurnRepository) : IInsertTimeTurnServices
{
    private readonly ILoggerServices _logger = logger;
    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public async Task Create(TimeTurn timeTurnViewModel)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Debug($"Horario {timeTurnViewModel.Id} creado");
            //});
            await _timeTurnRepository.CreateTT(timeTurnViewModel);
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
