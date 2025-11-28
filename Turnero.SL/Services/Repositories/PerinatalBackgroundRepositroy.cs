namespace Turnero.SL.Services.Repositories;

public class PerinatalBackgroundRepositroy(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<PerinatalBackground>(context, cache), IPerinatalBackgroundRepository
{
    public async Task<PerinatalBackground?> Get(Guid id)
    {
        return await FindByCondition(pb => pb.Id == id).FirstOrDefaultAsync();
    }
    public async Task Update(PerinatalBackground data)
    {
        var entity = await FindByCondition(pb => pb.Id == data.Id).FirstOrDefaultAsync();
        ArgumentNullException.ThrowIfNull(entity);
        await UpdateAsync(data);
    }
}

public interface IPerinatalBackgroundRepository
{
    Task<PerinatalBackground?> Get(Guid id);
    Task Update(PerinatalBackground data);
}