namespace Turnero.SL.Services.VisitServices;

public class GetVisitService(LoggerService logger, IVisitRepository visitRepository) : IGetVisitService
{
    private readonly LoggerService _logger = logger;
    private readonly IVisitRepository _visitRepository = visitRepository;

    public async Task<IQueryable<VisitDTO>> SearchVisits(Guid patientId)
    {
        try
        {
            return await _visitRepository.SearchVisits(patientId);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(SearchVisits)}: {ex.Message}");
            throw new Exception("An error occurred while searching for visits.");
        }
    }

    public async Task<Visit?> Get(Guid? id)
    {
        try
        {
            return await _visitRepository.Get(id);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return null;
        }
    }
}

public interface IGetVisitService
{
    Task<Visit?> Get(Guid? id);
    Task<IQueryable<VisitDTO>> SearchVisits(Guid patientId);
}
