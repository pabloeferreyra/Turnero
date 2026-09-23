namespace Turnero.Infrastructure.Repositories;

public class TimeTurnRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions)
    : RepositoryBase<TimeTurn>(context, cache, databaseOptions), ITimeTurnRepository
{
    private readonly ICachedQueryService cachedQueries = new CachedQueryService(cache);

    public async Task<List<TimeTurn>> GetList()
    {
        return await FindAll().OrderBy(t => t.Time).ToListAsync();
    }

    public IQueryable<TimeTurn> GetQueryable()
    {
        return FindAll().OrderBy(t => t.Time);
    }

    public async Task<TimeTurn> GetbyId(Guid id)
    {
        return await FindByCondition(t => t.Id == id).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No se encontró el turno con el id especificado.");
    }

    public bool Exists(Guid id)
    {
        return FindByCondition(t => t.Id == id).Any();
    }

    public async Task CreateTT(TimeTurn timeTurn)
    {
        await CreateAsync(timeTurn);
    }

    public void DeleteTT(TimeTurn timeTurn)
    {
        Delete(timeTurn);
    }

    public async Task<List<TimeTurn>> GetCachedTimes()
    {
        return await cachedQueries.GetCachedData("timeTurns", GetList);
    }
}
