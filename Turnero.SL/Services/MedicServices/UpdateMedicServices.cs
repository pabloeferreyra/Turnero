namespace Turnero.SL.Services.MedicServices;

public class UpdateMedicServices(LoggerService logger, IMedicRepository medicRepository) : IUpdateMedicServices
{
    private readonly LoggerService _logger = logger;
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task<bool> Update(Medic medic)
    {
        try
        {
            await _medicRepository.UpdateMedic(medic);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.Log(ex.Message);
            return false;
        }
    }

    public void Delete(Medic medic)
    {
        try
        {
            _medicRepository.DeleteMedic(medic);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.Log(ex.Message);
        }
    }
}

public interface IUpdateMedicServices
{
    Task<bool> Update(Medic medic);
    void Delete(Medic medic);
}