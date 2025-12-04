namespace Turnero.SL.Services.VaccinesServices;

public class UpdateVaccinesServices(LoggerService logger, IVaccinesRepository vaccinesRepository) : IUpdateVaccinesServices
{
    public async Task Update(VaccinesDto vaccines)
    {
        try
        {
            Vaccines vaccine = new()
            {
                Id = vaccines.Id,
                Description = vaccines.Description != "Otra" ? vaccines.Description : vaccines.OtherDescription,
                DateApplied = vaccines.DateApplied,
                PatientId = vaccines.PatientId
            };
            await vaccinesRepository.Update(vaccine);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in UpdateVaccines: {ex.Message}");
            throw;
        }
    }
}

public interface IUpdateVaccinesServices
{
    Task Update(VaccinesDto vaccines);
}
