namespace Turnero.SL.Services.HistoryServices;

public class UpdateHistoryService(LoggerService logger, IHistoryRepository historyRepository) : IUpdateHistoryService
{
    private readonly LoggerService _logger = logger;
    private readonly IHistoryRepository _historyRepository = historyRepository;
    public async Task UpdateHistory(History history)
    {
        try
        {
            await _historyRepository.UpdateHistory(history);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(UpdateHistory)}: {ex.Message}");
            throw new Exception("An error occurred while updating the patient's history.");
        }
    }
    public async Task DeleteHistory(Guid historyId)
    {
        try
        {
            var history = await _historyRepository.GetById(historyId) ?? throw new InvalidOperationException("History not found.");
            _historyRepository.DeleteHistory(history);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(DeleteHistory)}: {ex.Message}");
            throw new Exception("An error occurred while deleting the patient's history.");
        }
    }
}

public interface IUpdateHistoryService
{
    Task UpdateHistory(History history);
    Task DeleteHistory(Guid historyId);
}