namespace Turnero.SL.Services;

public class UpdateMedicServices(ILoggerServices logger, IMedicRepository medicRepository) : IUpdateMedicServices
{
    private readonly ILoggerServices _logger = logger;
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
            _logger.Error(ex.Message, ex);
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
            _logger.Error(ex.Message, ex);
        }
    }
}
