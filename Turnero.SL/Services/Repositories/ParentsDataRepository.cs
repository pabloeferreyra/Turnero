namespace Turnero.SL.Services.Repositories;

public class ParentsDataRepository (ApplicationDbContext context, IMemoryCache cache) 
    : RepositoryBase<ParentsData>(context, cache), IParentsDataRepository
{
    public async Task<ParentsData?> Get(Guid id)
    {
        return await FindByCondition(pd => pd.PatientId == id).SingleOrDefaultAsync();
    }
    public async Task Insert(ParentsData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        await CreateAsync(data);
    }
    public async Task Update(ParentsData data)
    {
        ArgumentNullException.ThrowIfNull(data);
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
