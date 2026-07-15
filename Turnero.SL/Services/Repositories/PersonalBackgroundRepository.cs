namespace Turnero.SL.Services.Repositories;

public class PersonalBackgroundRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<PersonalBackground>(context, cache), IPersonalBackgroundRepository
{
    public async Task<PersonalBackground?> Get(Guid id)
    {
        return await FindByCondition(pb => pb.Id == id).FirstOrDefaultAsync();
    }

    public async Task Update(PersonalBackground data)
    {
        var entity = await FindByCondition(pb => pb.Id == data.Id).FirstOrDefaultAsync();
        ArgumentNullException.ThrowIfNull(entity);
        await UpdateAsync(data);
    }
}

public interface IPersonalBackgroundRepository
{
    Task<PersonalBackground?> Get(Guid id);
    Task Update(PersonalBackground data);
}