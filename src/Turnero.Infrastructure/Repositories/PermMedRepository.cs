namespace Turnero.Infrastructure.Repositories;

public class PermMedRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : RepositoryBase<PermMed>(context, cache, databaseOptions), IPermMedRepository
{
    public async Task<List<PermMed>> GetByPatientId(Guid patientId)
    {
        return await FindByCondition(p => p.PatientId == patientId)
            .Include(p => p.Patient)
            .ToListAsync();
    }

    public async Task<PermMed?> GetById(Guid id)
    {
        return await FindByCondition(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task Insert(PermMed permMed)
    {
        Create(permMed);
    }
    public async Task Remove(Guid id)
    {
        if (!await FindByCondition(p => p.Id == id).AnyAsync())
            return;
        var stub = new PermMed { Id = id };
        _context.Set<PermMed>().Remove(stub);
        await _context.SaveChangesAsync();
    }
}

