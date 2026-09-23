namespace Turnero.Infrastructure.Repositories;

public class VaccinesRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : RepositoryBase<Vaccines>(context, cache, databaseOptions), IVaccinesRepository
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

    public new async Task Update(Vaccines vaccines)
    {
        await UpdateAsync(vaccines);
    }
    public async Task Insert(Vaccines vaccines)
    {
        Create(vaccines);
    }

    public void Remove(Vaccines vaccines)
    {
        Delete(vaccines);
    }
}

