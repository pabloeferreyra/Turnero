namespace Turnero.SL.Services.VaccinesServices;

public class UpdateVaccinesServices(LoggerService logger, IVaccinesRepository vaccinesRepository) : IUpdateVaccinesServices
{
    public async Task Update(Vaccines vaccines)
    {
        try
        {
            await vaccinesRepository.Update(vaccines);
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
    Task Update(Vaccines vaccines);
}
