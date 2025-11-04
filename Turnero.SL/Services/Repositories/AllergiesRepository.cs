namespace Turnero.SL.Services.Repositories;

public class AllergiesRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Allergies>(context, cache), IAllergiesRepository
{
    public async Task<Allergies?> Get(Guid? id)
    {
        return await FindByCondition(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAllergy(Allergies allergy)
    {
        await CreateAsync(allergy);
    }

    public async Task<List<Allergies>> GetAllergiesByPatient(Guid? id)
    {
        return await FindByCondition(a => a.PatientId == id).ToListAsync();
    }

    public async Task<IQueryable<Allergies>> SearchAllergies(Guid id)
    {
        var allergies = new List<Allergies>();
        if(id != Guid.Empty)
            allergies = await FindByCondition(a => a.PatientId == id)
                .Include(a => a.Patient).ToListAsync();
        return allergies.AsQueryable();
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

public interface IAllergiesRepository
{
    Task<Allergies?> Get(Guid? id);
    Task<List<Allergies>> GetAllergiesByPatient(Guid? id);
    Task CreateAllergy(Allergies allergy);
    Task UpdateAllergy(Allergies allergy);
    void DeleteAllergy(Allergies allergy);
    Task<IQueryable<Allergies>> SearchAllergies(Guid id);
}