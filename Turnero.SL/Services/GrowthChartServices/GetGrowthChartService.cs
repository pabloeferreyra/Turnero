namespace Turnero.SL.Services.GrowthChartServices;

public class GetGrowthChartService(IGrowthChartRepository repository) : IGetGrowthChartService
{
    public async Task<List<GrowthChart>> Get(Guid patientId)
    {
        return await repository.GetByPatientId(patientId);
    }
}

public interface IGetGrowthChartService
{
    Task<List<GrowthChart>> Get(Guid patientId);
}
