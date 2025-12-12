namespace Turnero.SL.Services.ParentsDataServices;

public class GetParentsDataService(IParentsDataRepository parentsDataRepository) : IGetParentsDataService
{
    public async Task<ParentsData?> GetParentsData(Guid id)
    {
        return await parentsDataRepository.Get(id);
    }
}
public interface IGetParentsDataService
{
    Task<ParentsData?> GetParentsData(Guid id);
}
