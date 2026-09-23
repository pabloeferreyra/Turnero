namespace Turnero.Infrastructure.Repositories;

public class AllergiesRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : RepositoryBase<Allergies>(context, cache, databaseOptions), IAllergiesRepository
{
    public async Task<Allergies?> Get(Guid? id)
    {
        return await FindByCondition(a => a.Id == id).Include(a => a.Patient).FirstOrDefaultAsync();
    }

    public async Task CreateAllergy(Allergies allergy)
    {
        await CreateAsync(allergy);
    }

    public async Task<List<Allergies>> GetAllergiesByPatient(Guid? id)
    {
        return await FindByCondition(a => a.PatientId == id)
            .Include(a => a.Patient)
            .ToListAsync();
    }

    public Task<IQueryable<Allergies>> SearchAllergies(Guid id)
    {
        if (id == Guid.Empty)
            return Task.FromResult(Enumerable.Empty<Allergies>().AsQueryable());
        IQueryable<Allergies> query = FindByCondition(a => a.PatientId == id)
            .Include(a => a.Patient);
        return Task.FromResult(query);
    }

    public async Task UpdateAllergy(Allergies allergy)
    {
        await UpdateAsync(allergy);
    }
    public void DeleteAllergy(Allergies allergy)
    {
        Delete(allergy);
    }
}

