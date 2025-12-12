namespace Turnero.SL.Services.Repositories;

public class GrowthChartRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<GrowthChart>(context, cache), IGrowthChartRepository
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
        await CreateAsync(growthChart);
    }
    public async Task Edit(GrowthChart growthChart)
    {
        await UpdateAsync(growthChart);
    }
    public async Task Remove(Guid id)
    {
        var growthChart = await FindByCondition(g => g.Id == id).FirstOrDefaultAsync();
        Delete(growthChart);
    }
}
public interface IGrowthChartRepository
{
    Task<List<GrowthChart>> GetByPatientId(Guid patientId);
    Task<GrowthChart?> GetById(Guid id);
    Task Insert(GrowthChart growthChart);
    Task Edit(GrowthChart growthChart);
    Task Remove(Guid id);
}
