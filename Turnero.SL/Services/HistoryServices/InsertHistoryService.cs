namespace Turnero.SL.Services.HistoryServices;

public class InsertHistoryService(LoggerService logger, IHistoryRepository historyRepository) : IInsertHistoryService
{
    private readonly IHistoryRepository _historyRepository = historyRepository;
    private readonly LoggerService _logger = logger;

    public async Task InsertHistory(History history)
    {
        try
        {
            await _historyRepository.CreateHistory(history);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(InsertHistory)}: {ex.Message}");
            throw new Exception("An error occurred while inserting the history.");
        }
    }
}

public interface IInsertHistoryService
{
    Task InsertHistory(History history);
}
