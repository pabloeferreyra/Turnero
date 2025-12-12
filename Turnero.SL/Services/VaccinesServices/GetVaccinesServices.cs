namespace Turnero.SL.Services.VaccinesServices;

public class GetVaccinesServices(LoggerService logger, IVaccinesRepository vaccinesRepository) : IGetVaccinesServices
{
    public async Task<Vaccines?> Get(Guid? id)
    {
        try
        {
            var vaccines = await vaccinesRepository.Get(id);
            return vaccines;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in GetVaccines: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Vaccines>> GetByPatientId(Guid patientId)
    {
        try
        {
            var vaccines = await vaccinesRepository.GetByPatientId(patientId);
            return vaccines;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in GetByPatientId: {ex.Message}");
            throw;
        }
    }
}

public interface IGetVaccinesServices
{
    Task<Vaccines?> Get(Guid? id);
    Task<List<Vaccines>> GetByPatientId(Guid patientId);
}
