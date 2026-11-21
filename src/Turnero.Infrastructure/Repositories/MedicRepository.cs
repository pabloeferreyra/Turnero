namespace Turnero.Infrastructure.Repositories;

public class MedicRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions)
    : RepositoryBase<Medic>(context, cache, databaseOptions), IMedicRepository
{
    private readonly ICachedQueryService cachedQueries = new CachedQueryService(cache);

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
        return await cachedQueries.GetCachedData("medics", GetListDto);
    }

    public void InvalidateCachedMedics()
    {
        _cache.Remove("medics");
    }

    public async Task<List<MedicAvailability>> GetAvailability(Guid id, DateOnly date)
    {
        var day = date.DayOfWeek;
        return await _context.MedicAvailabilities
            .AsNoTracking()
            .Where(availability => availability.MedicId == id
                && availability.DateFrom <= date
                && availability.DateTo >= date
                && availability.Day == day)
            .Include(availability => availability.TimeFrom)
            .Include(availability => availability.TimeTo)
            .ToListAsync();
    }

    public async Task<List<MedicAvailability>> GetAvailabilities(Guid id)
    {
        return await _context.MedicAvailabilities
            .AsNoTracking()
            .Where(availability => availability.MedicId == id)
            .OrderBy(availability => availability.DateFrom)
            .ThenBy(availability => availability.Day)
            .ToListAsync();
    }

    public async Task CreateAvailability(MedicAvailability availability)
    {
        await _context.MedicAvailabilities.AddAsync(availability);
        await _context.SaveChangesAsync();
    }

    public async Task SaveAvailabilities(
        Guid medicId,
        IReadOnlyCollection<MedicAvailability> availabilities,
        IReadOnlyCollection<Guid> unavailableIds)
    {
        var requestedIds = availabilities
            .Select(availability => availability.Id)
            .Concat(unavailableIds)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var existing = await _context.MedicAvailabilities
            .Where(availability =>
                availability.MedicId == medicId
                && requestedIds.Contains(availability.Id))
            .ToDictionaryAsync(availability => availability.Id);

        foreach (var id in unavailableIds)
        {
            if (existing.TryGetValue(id, out var unavailable))
            {
                _context.MedicAvailabilities.Remove(unavailable);
            }
        }

        foreach (var availability in availabilities)
        {
            if (availability.Id != Guid.Empty
                && existing.TryGetValue(availability.Id, out var current))
            {
                current.DateFrom = availability.DateFrom;
                current.DateTo = availability.DateTo;
                current.Day = availability.Day;
                current.TimeFromId = availability.TimeFromId;
                current.TimeToId = availability.TimeToId;
                continue;
            }

            availability.Id = Guid.NewGuid();
            availability.MedicId = medicId;
            await _context.MedicAvailabilities.AddAsync(availability);
        }

        await _context.SaveChangesAsync();
    }
}
