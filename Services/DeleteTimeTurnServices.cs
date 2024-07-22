namespace Turnero.Services;

public class DeleteTimeTurnServices(
                              ITimeTurnRepository timeTurnRepository) : IDeleteTimeTurnServices
{
    private readonly ITimeTurnRepository _timeTurnRepository = timeTurnRepository;

    public void Delete(TimeTurn timeTurn)
    {
        try
        {
            _timeTurnRepository.DeleteTT(timeTurn);
        }
        catch (Exception)
        {
        }
    }
}
