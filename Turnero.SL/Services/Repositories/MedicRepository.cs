using AutoMapper.QueryableExtensions;

namespace Turnero.SL.Services.Repositories;

public class MedicRepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : RepositoryBase<Medic>(context, mapper, cache), IMedicRepository
{
    public IMapper mapper = mapper;

    public async Task<List<MedicDto>> GetListDto()
    {
        return await FindAll().ProjectTo<MedicDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<List<Medic>> GetList()
    {
        return await FindAll().ToListAsync();
    }

    public async Task<Medic> GetById(Guid id)
    {
        return await FindByCondition(m => m.Id == id).SingleOrDefaultAsync();
    }

    public async Task<Medic> GetByUserId(string id)
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
        DeleteAsync(medic);
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
