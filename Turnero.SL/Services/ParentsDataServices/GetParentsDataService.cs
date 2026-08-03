namespace Turnero.SL.Services.ParentsDataServices;

public class GetParentsDataService(IParentsDataRepository parentsDataRepository) : IGetParentsDataService
{
    public async Task<ParentsData?> GetParentsData(Guid id)
    {
       
        var data = await parentsDataRepository.Get(id);
        return data;
    }
}
public interface IGetParentsDataService
{
    Task<ParentsData?> GetParentsData(Guid id);
}
