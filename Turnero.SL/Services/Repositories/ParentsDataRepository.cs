namespace Turnero.SL.Services.Repositories;

public class ParentsDataRepository(ApplicationDbContext context, IMemoryCache cache)
    : RepositoryBase<ParentsData>(context, cache), IParentsDataRepository
{
    public async Task<ParentsData?> Get(Guid id)
    {
        return await FindByCondition(pd => pd.Id == id).Include(p => p.Patient).SingleOrDefaultAsync();
    }
    public async Task Insert(ParentsData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        await CreateAsync(data);
    }
    public async Task Update(ParentsData data)
    {
        var entity = await FindByCondition(pd => pd.Id == data.Id).Include(p => p.Patient).SingleOrDefaultAsync();
        ArgumentNullException.ThrowIfNull(entity);
        await UpdateAsync(data);
    }
    public void Delete(ParentsData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Delete(data);
    }
}

public interface IParentsDataRepository
{
    Task<ParentsData?> Get(Guid id);
    Task Insert(ParentsData data);
    Task Update(ParentsData data);
    void Delete(ParentsData data);
}
