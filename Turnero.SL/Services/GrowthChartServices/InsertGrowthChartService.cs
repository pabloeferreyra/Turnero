namespace Turnero.SL.Services.GrowthChartServices;

public class InsertGrowthChartService(IGrowthChartRepository repository) : IInsertGrowthChartService
{
    public async Task Create(GrowthChart growthChart)
    {
        await repository.Insert(growthChart);
    }
}

public interface IInsertGrowthChartService
{
    Task Create(GrowthChart growthChart);
}
