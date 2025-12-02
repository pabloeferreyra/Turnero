namespace Turnero.SL.Services.VaccinesServices;

public class InsertVaccinesServices(LoggerService logger, IVaccinesRepository vaccinesRepository) : IInsertVaccinesServices
{
    public async Task Insert(Vaccines vaccines)
    {
        try
        {
            await vaccinesRepository.Insert(vaccines);
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
    Task Insert(Vaccines vaccines);
}
