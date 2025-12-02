namespace Turnero.SL.Services.Repositories;

public class VaccinesRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Vaccines>(context, cache), IVaccinesRepository
{
    public async Task<Vaccines?> Get(Guid? id)
    {
        return await FindByCondition(v => v.Id == id)
            .Include(v => v.Patient)
            .SingleOrDefaultAsync();
    }
    public async Task<List<Vaccines>> GetByPatientId(Guid patientId)
    {
        return await FindByCondition(v => v.PatientId == patientId)
            .Include(v => v.Patient)
            .ToListAsync();
    }

    public async Task Update(Vaccines vaccines)
    {
        await UpdateAsync(vaccines);
    }
    public async Task Insert(Vaccines vaccines)
    {
        Create(vaccines);
    }
}

public interface IVaccinesRepository
{
    Task<Vaccines?> Get(Guid? id);
    Task<List<Vaccines>> GetByPatientId(Guid patientId);
    Task Update(Vaccines vaccines);
    Task Insert(Vaccines vaccines);
}