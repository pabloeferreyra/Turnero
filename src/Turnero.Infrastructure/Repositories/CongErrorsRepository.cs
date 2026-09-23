namespace Turnero.Infrastructure.Repositories;

public class CongErrorsRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions)
    : RepositoryBase<CongErrors>(context, cache, databaseOptions), ICongErrorsRepository
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

