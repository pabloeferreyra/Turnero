namespace Turnero.SL.Services.Repositories;

public class VisitRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Visit>(context, cache), IVisitRepository
{
    public async Task<Visit?> Get(Guid? id)
    {
        return await FindByCondition(v => v.Id == id)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .SingleOrDefaultAsync();
    }

    public async Task<IQueryable<VisitDTO>> SearchVisits(Guid patientId)
    {
        if (patientId == Guid.Empty)
            return FindAll()
                .Include(v => v.Patient).Include(v => v.Medic)
                .Adapt<List<VisitDTO>>()
                .AsQueryable();
        return FindByCondition(v => v.PatientId == patientId)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToList()
            .Adapt<List<VisitDTO>>()
            .AsQueryable();
    }

    public async Task CreateVisit(Visit visit)
    {
        await CreateAsync(visit);
    }
}

public interface IVisitRepository
{
    Task<Visit?> Get(Guid? id);
    Task<IQueryable<VisitDTO>> SearchVisits(Guid patientId);
    Task CreateVisit(Visit visit);
}
