namespace Turnero.SL.Services.TurnsServices;

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

public interface IDeleteTimeTurnServices
{
    void Delete(TimeTurn timeTurn);
}

