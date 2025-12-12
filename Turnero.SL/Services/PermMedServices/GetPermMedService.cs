namespace Turnero.SL.Services.PermMedServices;

public class GetPermMedService(IPermMedRepository permMedRepository) : IGetPermMedService
{
    public async Task<List<PermMed>> Get(Guid patientId)
    {
        return await permMedRepository.GetByPatientId(patientId);
    }
}
public interface IGetPermMedService
{
    Task<List<PermMed>> Get(Guid patientId);
}
