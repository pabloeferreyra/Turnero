namespace Turnero.Infrastructure.Repositories;

public class GrowthChartRepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : RepositoryBase<GrowthChart>(context, cache, databaseOptions), IGrowthChartRepository
{
    public async Task<List<GrowthChart>> GetByPatientId(Guid patientId)
    {
        return await FindByCondition(g => g.PatientId == patientId)
            .Include(g => g.Patient)
            .ToListAsync();
    }

    public async Task<GrowthChart?> GetById(Guid id)
    {
        return await FindByCondition(g => g.Id == id)
            .Include(g => g.Patient)
            .FirstOrDefaultAsync();
    }

    public async Task Insert(GrowthChart growthChart)
    {
        growthChart.Id = Guid.NewGuid();
        await CreateAsync(growthChart);
    }
    public async Task Edit(GrowthChart growthChart)
    {
        await UpdateAsync(growthChart);
    }
    public async Task Remove(Guid id)
    {
        if (!await FindByCondition(g => g.Id == id).AnyAsync())
            return;
        var stub = new GrowthChart { Id = id };
        _context.Set<GrowthChart>().Remove(stub);
        await _context.SaveChangesAsync();
    }
}
