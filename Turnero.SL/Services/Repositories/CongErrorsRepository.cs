namespace Turnero.SL.Services.Repositories;

public class CongErrorsRepository(ApplicationDbContext context, IMemoryCache cache, RedisCacheService redisCache)
    : RepositoryBase<CongErrors>(context, cache, redisCache), ICongErrorsRepository
{
    public async Task<CongErrors?> Get(Guid id)
    {
        return await FindByCondition(ce => ce.Id == id).Include(p => p.Patient).SingleOrDefaultAsync();
    }
    public new async Task Update(CongErrors data)
    {
        if (!await FindByCondition(ce => ce.Id == data.Id).AnyAsync())
            throw new ArgumentNullException(nameof(data));
        await UpdateAsync(data);
    }
}

public interface ICongErrorsRepository
{
    Task<CongErrors?> Get(Guid id);
    Task Update(CongErrors data);
}
