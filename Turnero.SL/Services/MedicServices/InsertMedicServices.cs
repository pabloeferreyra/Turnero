namespace Turnero.SL.Services.MedicServices;

public class InsertMedicServices(LoggerService logger, IMedicRepository medicRepository) : IInsertMedicServices
{
    private readonly LoggerService _logger = logger;
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task Create(Medic medic)
    {
        try
        {
            await _medicRepository.NewMedic(medic);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
        }
    }
}

public interface IInsertMedicServices
{
    Task Create(Medic medic);
}
