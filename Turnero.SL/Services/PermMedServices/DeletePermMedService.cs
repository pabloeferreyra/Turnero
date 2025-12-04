namespace Turnero.SL.Services.PermMedServices;

public class DeletePermMedService(IPermMedRepository permMedRepository) : IDeletePermMedService
{
    public async Task Delete(Guid id)
    {
        await permMedRepository.Remove(id);
    }
}

public interface IDeletePermMedService
{
    Task Delete(Guid id);
}
