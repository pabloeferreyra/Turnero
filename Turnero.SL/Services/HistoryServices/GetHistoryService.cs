namespace Turnero.SL.Services.HistoryServices;

public class GetHistoryService(LoggerService logger, IHistoryRepository historyRepository) : IGetHistoryService
{
    private readonly LoggerService _logger = logger;
    private readonly IHistoryRepository _historyRepository = historyRepository;
    public async Task<History?> GetByPatientId(Guid patientId)
    {
        try
        {
            return await _historyRepository.GetByPatientId(patientId);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(GetByPatientId)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the patient's history.");
        }
    }
}

public interface IGetHistoryService
{
    Task<History?> GetByPatientId(Guid patientId);
}
