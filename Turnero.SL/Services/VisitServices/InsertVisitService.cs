namespace Turnero.SL.Services.VisitServices;

public class InsertVisitService(LoggerService logger, IVisitRepository visitRepository) : IInsertVisitService
{
    private readonly LoggerService _logger = logger;
    private readonly IVisitRepository _visitRepository = visitRepository;
    public async Task Create(Visit visit)
    {
        try
        {
            await _visitRepository.CreateVisit(visit);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
        }
    }
}

public interface IInsertVisitService
{
    Task Create(Visit visit);
}