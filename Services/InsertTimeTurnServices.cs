namespace Turnero.Services;

public class InsertTimeTurnServices(ITimeTurnRepository timeTurnRepository) : IInsertTimeTurnServices
{

    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public async Task Create(TimeTurn timeTurnViewModel)
    {
        try
        {
            await _timeTurnRepository.CreateTT(timeTurnViewModel);
        }
        catch (Exception)
        {
        }
    }
}
