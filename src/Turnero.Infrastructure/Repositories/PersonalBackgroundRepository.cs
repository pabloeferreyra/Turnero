namespace Turnero.Infrastructure.Repositories;

public class PersonalBackgroundRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : RepositoryBase<PersonalBackground>(context, cache, databaseOptions), IPersonalBackgroundRepository
{
    public async Task<PersonalBackground?> Get(Guid id)
    {
        return await FindByCondition(pb => pb.Id == id).FirstOrDefaultAsync();
    }

    public new async Task Update(PersonalBackground data)
    {
        if (!await FindByCondition(pb => pb.Id == data.Id).AnyAsync())
            throw new ArgumentNullException(nameof(data));
        await UpdateAsync(data);
    }
}

