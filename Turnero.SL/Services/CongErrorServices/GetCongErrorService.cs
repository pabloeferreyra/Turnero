namespace Turnero.SL.Services.CongErrorServices;

public class GetCongErrorService(LoggerService logger, ICongErrorsRepository repository) : IGetCongErrorService
{
    public async Task<CongErrors> GetCongError(Guid id)
    {
        try
        {
            var congError = await repository.Get(id) ?? throw new InvalidOperationException($"CongError with id {id} was not found.");
            return congError;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(GetCongError)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the CongError.");
        }
    }
}

public interface IGetCongErrorService
{
    Task<CongErrors> GetCongError(Guid id);
}
