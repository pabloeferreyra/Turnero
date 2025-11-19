namespace Turnero.SL.Services.Repositories;

public class PersonalBackgroundRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<PersonalBackground>(context, cache), IPersonalBackgroundRepository
{
    public void Delete(PersonalBackground data)
    {
       ArgumentNullException.ThrowIfNull(data);
        Delete(data);
    }

    public async Task<PersonalBackground?> Get(Guid id)
    {
        return await FindByCondition(pb => pb.Id == id).FirstOrDefaultAsync();
    }

    public async Task Insert(PersonalBackground data)
    {
        ArgumentNullException.ThrowIfNull(data);
        await CreateAsync(data);
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
    Task Insert(PersonalBackground data);
    Task Update(PersonalBackground data);
    void Delete(PersonalBackground data);
}