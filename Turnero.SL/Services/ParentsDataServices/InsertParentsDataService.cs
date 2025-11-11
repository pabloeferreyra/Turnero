namespace Turnero.SL.Services.ParentsDataServices;

public class InsertParentsDataService (LoggerService logger, IParentsDataRepository parentsDataRepository) : IInsertParentsDataService
{
    public async Task InsertParentsData(ParentsData data)
    {
        try
        {
            await parentsDataRepository.Insert(data);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(InsertParentsData)}: {ex.Message}");
            throw new Exception("An error occurred while inserting parents data.");
        }
    }
}
public interface IInsertParentsDataService
{
    Task InsertParentsData(ParentsData data);
}
