namespace Turnero.SL.Services.VaccinesServices;

public class DeleteVacinesServices(LoggerService logger, IVaccinesRepository vaccinesRepository) : IDeleteVacinesServices
{
    public async Task Delete(Guid id)
    {
        try
        {
            var vaccine = await vaccinesRepository.Get(id) ?? throw new Exception("Vaccine not found");
            vaccinesRepository.Remove(vaccine);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in DeleteVaccines: {ex.Message}");
            throw;
        }
    }
}
public interface IDeleteVacinesServices
{
    Task Delete(Guid id);
}
