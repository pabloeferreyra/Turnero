namespace Turnero.SL.Services.VaccinesServices;

public class InsertVaccinesServices(LoggerService logger, IVaccinesRepository vaccinesRepository) : IInsertVaccinesServices
{
    public async Task Insert(VaccinesDto vaccines)
    {
        try
        {
            Vaccines vaccine = new()
            {
                Description = vaccines.Description != "Otra" ? vaccines.Description : vaccines.OtherDescription,
                DateApplied = vaccines.DateApplied,
                PatientId = vaccines.PatientId
            };
            await vaccinesRepository.Insert(vaccine);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in InsertVaccines: {ex.Message}");
            throw;
        }
    }
}
public interface IInsertVaccinesServices
{
    Task Insert(VaccinesDto vaccines);
}
