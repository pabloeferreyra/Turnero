namespace Turnero.SL.Services.GrowthChartServices;

public class InsertGrowthChartService(IGrowthChartRepository repository) : IInsertGrowthChartService
{
    public async Task Insert(GrowthChart growthChart)
    {
        await repository.Insert(growthChart);
    }
}

public interface IInsertGrowthChartService
{
    Task Insert(GrowthChart growthChart);
}
