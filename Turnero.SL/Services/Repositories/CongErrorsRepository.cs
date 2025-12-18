namespace Turnero.SL.Services.Repositories;

public class CongErrorsRepository(ApplicationDbContext context, IMemoryCache cache)
    : RepositoryBase<CongErrors>(context, cache), ICongErrorsRepository
{
    public async Task<CongErrors?> Get(Guid id)
    {
        return await FindByCondition(ce => ce.Id == id).Include(p => p.Patient).SingleOrDefaultAsync();
    }
    public async Task Insert(CongErrors data)
    {
        ArgumentNullException.ThrowIfNull(data);
        await CreateAsync(data);
    }
    public async Task Update(CongErrors data)
    {
        var entity = await FindByCondition(ce => ce.Id == data.Id).Include(p => p.Patient).SingleOrDefaultAsync();
        ArgumentNullException.ThrowIfNull(entity);
        await UpdateAsync(data);
    }
    public void Delete(CongErrors data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Delete(data);
    }
}

public interface ICongErrorsRepository
{
    Task<CongErrors?> Get(Guid id);
    Task Insert(CongErrors data);
    Task Update(CongErrors data);
    void Delete(CongErrors data);
}
