namespace Turnero.SL.Services;

public class DeleteTimeTurnServices(ITimeTurnRepository timeTurnRepository) : IDeleteTimeTurnServices
{
    public void Delete(TimeTurn timeTurn)
    {
        try
        {
            timeTurnRepository.DeleteTT(timeTurn);
            //_logger.Info($"Tiempo {timeTurn.Id} eliminado correctamente");
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
