namespace Turnero.SL.Services.CongErrorServices;

public class InsertCongErrorService(LoggerService logger, ICongErrorsRepository repository) : IInsertCongErrorService
{
    public async Task InsertCongError(CongErrors congError)
    {
        try
        {
            await repository.Insert(congError);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(InsertCongError)}: {ex.Message}");
            throw new Exception("An error occurred while inserting the CongError.");
        }
    }
}

public interface IInsertCongErrorService
{
    Task InsertCongError(CongErrors congError);
}
