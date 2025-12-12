namespace Turnero.SL.Services.PermMedServices;

public class InsertPermMedService(IPermMedRepository permMedRepository) : IInsertPermMedService
{
    public async Task Create(PermMed permMed)
    {
        await permMedRepository.Insert(permMed);
    }
}

public interface IInsertPermMedService
{
    Task Create(PermMed permMed);
}
