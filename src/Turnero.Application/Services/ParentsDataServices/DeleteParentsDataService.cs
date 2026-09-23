namespace Turnero.Application.Services.ParentsDataServices;

public class DeleteParentsDataService(IParentsDataRepository parentsDataRepository) : IDeleteParentsDataService
{
    public void DeleteParentsData(ParentsData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        parentsDataRepository.Delete(data);
    }
}
public interface IDeleteParentsDataService
{
    void DeleteParentsData(ParentsData data);
}
