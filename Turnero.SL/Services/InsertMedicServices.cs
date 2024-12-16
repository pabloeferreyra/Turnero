namespace Turnero.SL.Services;

public class InsertMedicServices(ILoggerServices logger, IMedicRepository medicRepository) : IInsertMedicServices
{
    private readonly ILoggerServices _logger = logger;
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task Create(Medic medic)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Info($"Creado Medico {medic.Id}");
            //});
            await _medicRepository.NewMedic(medic);
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
