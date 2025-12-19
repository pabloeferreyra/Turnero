namespace Turnero.SL.Services.CongErrorServices;

public class DeleteCongErrorService(LoggerService logger, ICongErrorsRepository repository) : IDeleteCongErrorService
{
    public void DeleteCongError(CongErrors congErrors)
    {
        try
        {
            repository.Delete(congErrors);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(DeleteCongError)}: {ex.Message}");
            throw new Exception("An error occurred while deleting the CongError.");
        }
    }
}

public interface IDeleteCongErrorService
{
    void DeleteCongError(CongErrors congErrors);
}
