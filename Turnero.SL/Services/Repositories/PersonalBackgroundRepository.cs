namespace Turnero.SL.Services.Repositories;

public class PersonalBackgroundRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<PersonalBackground>(context, cache), IPersonalBackgroundRepository
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

public interface IPersonalBackgroundRepository
{
    Task<PersonalBackground?> Get(Guid id);
    Task Update(PersonalBackground data);
}