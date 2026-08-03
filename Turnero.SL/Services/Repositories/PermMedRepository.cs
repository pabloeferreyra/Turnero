namespace Turnero.SL.Services.Repositories;

public class PermMedRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<PermMed>(context, cache), IPermMedRepository
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

public interface IPermMedRepository
{
    Task<List<PermMed>> GetByPatientId(Guid patientId);
    Task<PermMed?> GetById(Guid id);
    Task Insert(PermMed permMed);
    Task Remove(Guid id);
}
