namespace Turnero.SL.Services.Repositories;

public class VisitRepository(ApplicationDbContext context, IMemoryCache cache, RedisCacheService redisCache) : RepositoryBase<Visit>(context, cache, redisCache), IVisitRepository
{
    public async Task<Visit?> Get(Guid? id)
    {
        return await FindByCondition(v => v.Id == id)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .SingleOrDefaultAsync();
    }

    public Task<IQueryable<VisitDTO>> SearchVisits(Guid patientId)
    {
        if (patientId == Guid.Empty)
            return Task.FromResult(FindAll().ProjectToType<VisitDTO>());
        return Task.FromResult(FindByCondition(v => v.PatientId == patientId).ProjectToType<VisitDTO>());
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
