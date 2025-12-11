namespace Turnero.SL.Services.GrowthChartServices;

public class GetGrowthChartService(IGrowthChartRepository repository) : IGetGrowthChartService
{
    public async Task<List<GrowthChart>> Get(Guid patientId)
    {
        return await repository.GetByPatientId(patientId);
    }
    public async Task<GrowthChart?> GetById(Guid id)
    {
        return await repository.GetById(id);
    }
}

public interface IGetGrowthChartService
{
    Task<List<GrowthChart>> Get(Guid patientId);
    Task<GrowthChart?> GetById(Guid id);
}
