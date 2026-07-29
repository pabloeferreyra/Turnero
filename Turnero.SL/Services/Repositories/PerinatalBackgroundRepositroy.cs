namespace Turnero.SL.Services.Repositories;

public class PerinatalBackgroundRepositroy(ApplicationDbContext context, IMemoryCache cache, RedisCacheService redisCache) : RepositoryBase<PerinatalBackground>(context, cache, redisCache), IPerinatalBackgroundRepository
{
    public async Task<PerinatalBackground?> Get(Guid id)
    {
        return await FindByCondition(pb => pb.Id == id).FirstOrDefaultAsync();
    }
    public new async Task Update(PerinatalBackground data)
    {
        if (!await FindByCondition(pb => pb.Id == data.Id).AnyAsync())
            throw new ArgumentNullException(nameof(data));
        await UpdateAsync(data);
    }
}

public interface IPerinatalBackgroundRepository
{
    Task<PerinatalBackground?> Get(Guid id);
    Task Update(PerinatalBackground data);
}