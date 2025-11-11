namespace Turnero.SL.Services.ParentsDataServices;

public class UpdateParentsDataService(LoggerService logger, IParentsDataRepository parentsDataRepository) : IUpdateParentsDataService
{
    public async Task UpdateParentsData(ParentsData data)
    {
        try
        {
            await parentsDataRepository.Update(data);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(UpdateParentsData)}: {ex.Message}");
            throw new Exception("An error occurred while updating parents data.");
        }
    }
}
public interface IUpdateParentsDataService
{
    Task UpdateParentsData(ParentsData data);
}
