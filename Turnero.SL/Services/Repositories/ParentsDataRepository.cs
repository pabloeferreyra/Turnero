namespace Turnero.SL.Services.Repositories;

public class ParentsDataRepository(ApplicationDbContext context, IMemoryCache cache)
    : RepositoryBase<ParentsData>(context, cache), IParentsDataRepository
{
    public async Task<ParentsData?> Get(Guid id)
    {
        return await FindByCondition(pd => pd.Id == id).Include(p => p.Patient).SingleOrDefaultAsync();
    }
    public new async Task Update(ParentsData data)
    {
        if (!await FindByCondition(pd => pd.Id == data.Id).AnyAsync())
            throw new ArgumentNullException(nameof(data));
        await UpdateAsync(data);
    }
    public new void Delete(ParentsData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Delete(data);
    }
}

public interface IParentsDataRepository
{
    Task<ParentsData?> Get(Guid id);
    Task Update(ParentsData data);
    void Delete(ParentsData data);
}
