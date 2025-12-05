namespace Turnero.SL.Services.GrowthChartServices;

public class UpdateGrowthChartService(IGrowthChartRepository repository) : IUpdateGrowthChartService
{
    public async Task Edit(GrowthChart growthChart)
    {
        await repository.Edit(growthChart);
    }
}
public interface IUpdateGrowthChartService
{
    Task Edit(GrowthChart growthChart);
}
