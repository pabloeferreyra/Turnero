
namespace Turnero.Services;

public class GetAvailableService(IAvailableRepository repository) : IGetAvailableService
{
    private readonly IAvailableRepository _repository = repository;

    public Available GetAvailable(Guid id)
    {
        try
        {
            return _repository.GetById(id);
        }
        catch (Exception) 
        { 
            return null; 
        }
    }

    public List<Available> GetAvailables()
    {
        return _repository.GetAvailables();
    }

    public List<Available> GetAvailablesForMedic(Guid id)
    {
        try
        {
            return _repository.GetByMedic(id);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
