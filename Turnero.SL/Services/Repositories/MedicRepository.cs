namespace Turnero.SL.Services.Repositories;

public class MedicRepository(ApplicationDbContext context, IMemoryCache cache, RedisCacheService redisCache) : RepositoryBase<Medic>(context, cache, redisCache), IMedicRepository
{
    public async Task<List<MedicDto>> GetListDto()
    {
        return await FindAll().ProjectToType<MedicDto>().ToListAsync();
    }

    public async Task<List<Medic>> GetList()
    {
        return await FindAll().ToListAsync();
    }

    public async Task<Medic> GetById(Guid id)
    {
        return await FindByCondition(m => m.Id == id).SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("No se encontró el médico con el id especificado.");
    }

    public async Task<Medic?> GetByUserId(string id)
    {
        return await FindByCondition(m => m.UserGuid == id).SingleOrDefaultAsync();
    }

    public bool Exists(Guid id)
    {
        return FindByCondition(m => m.Id == id).Any();
    }

    public async Task NewMedic(Medic medic)
    {
        if (!string.IsNullOrEmpty(medic.Name))
        {
            await CreateAsync(medic);
        }
    }

    public void DeleteMedic(Medic medic)
    {
        Delete(medic);
    }

    public async Task UpdateMedic(Medic medic)
    {
        await UpdateAsync(medic);
    }

    public async Task<List<MedicDto>> GetCachedMedics()
    {
        return await GetCachedData("medics", GetListDto);
    }
}
