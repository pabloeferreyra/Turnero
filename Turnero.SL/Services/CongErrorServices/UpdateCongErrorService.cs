namespace Turnero.SL.Services.CongErrorServices;

public class UpdateCongErrorService(LoggerService logger, ICongErrorsRepository repository) : IUpdateCongErrorService
{
    public async Task UpdateCongError(CongErrors congError)
    {
        try
        {
            await repository.Update(congError);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(UpdateCongError)}: {ex.Message}");
            throw new Exception("An error occurred while updating the CongError.");
        }
    }
}
public interface IUpdateCongErrorService
{
    Task UpdateCongError(CongErrors congError);
}
