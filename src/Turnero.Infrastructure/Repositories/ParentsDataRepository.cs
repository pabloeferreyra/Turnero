namespace Turnero.Infrastructure.Repositories;

public class ParentsDataRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions)
    : RepositoryBase<ParentsData>(context, cache, databaseOptions), IParentsDataRepository
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

