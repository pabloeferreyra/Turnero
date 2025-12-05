namespace Turnero.SL.Services.Repositories;

public class PermMedRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<PermMed>(context, cache), IPermMedRepository
{
    public async Task<List<PermMed>> GetByPatientId(Guid patientId)
    {
        return await FindByCondition(p => p.PatientId == patientId)
            .Include(p => p.Patient)
            .ToListAsync();
    }
    public async Task Insert(PermMed permMed)
    {
        Create(permMed);
    }
    public async Task Remove(Guid id)
    {
        var permMed = await FindByCondition(p => p.Id == id).FirstOrDefaultAsync();
        Delete(permMed);
    }
}

public interface IPermMedRepository
{
    Task<List<PermMed>> GetByPatientId(Guid patientId);
    Task Insert(PermMed permMed);
    Task Remove(Guid id);
}
