namespace Turnero.SL.Services.GrowthChartServices;

public class DeleteGrowthChartService(IGrowthChartRepository repository) : IDeleteGrowthChartService
{
    public async Task Delete(Guid id)
    {
        await repository.Remove(id);
    }
}
public interface IDeleteGrowthChartService
{
    public Task Delete(Guid id);
}
