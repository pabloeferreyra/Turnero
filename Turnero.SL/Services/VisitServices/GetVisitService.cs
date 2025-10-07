namespace Turnero.SL.Services.VisitServices;

public class GetVisitService(LoggerService logger, IVisitRepository visitRepository) : IGetVisitService
{
    private readonly LoggerService _logger = logger;
    private readonly IVisitRepository _visitRepository = visitRepository;

    public async Task<List<Visit>> GetVisitsByMedicAndDate(Guid medicId, DateTime date)
    {
        try
        {
            return await _visitRepository.GetVisitsByMedicAndDate(medicId, date);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByPatient(Guid patientId)
    {
        try
        {
            return await _visitRepository.GetVisitsByPatient(patientId);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetAllVisits()
    {
        try
        {
            return await _visitRepository.GetAllVisits();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByDate(DateTime date)
    {
        try
        {
            return await _visitRepository.GetVisitsByDate(date);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByMedic(Guid medicId)
    {
        try
        {
            return await _visitRepository.GetVisitsByMedic(medicId);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByDateRange(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _visitRepository.GetVisitsByDateRange(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _visitRepository.GetVisitsByMedicAndDateRange(medicId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public Task<List<Visit>> GetVisitsByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return _visitRepository.GetVisitsByPatientAndDateRange(patientId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return Task.FromResult(new List<Visit>());
        }
    }
    public async Task<int> GetVisitCountByMedicAndDate(Guid medicId, DateTime date)
    {
        try
        {
            return await _visitRepository.GetVisitCountByMedicAndDate(medicId, date);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetVisitCountByPatient(Guid patientId)
    {
        try
        {
            return await _visitRepository.GetVisitCountByPatient(patientId);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetTotalVisitCount()
    {
        try
        {
            return await _visitRepository.GetTotalVisitCount();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetVisitCountByDate(DateTime date)
    {
        try
        {
            return await _visitRepository.GetVisitCountByDate(date);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetVisitCountByMedic(Guid medicId)
    {
        try
        {
            return await _visitRepository.GetVisitCountByMedic(medicId);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetVisitCountByDateRange(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _visitRepository.GetVisitCountByDateRange(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetVisitCountByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _visitRepository.GetVisitCountByMedicAndDateRange(medicId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<int> GetVisitCountByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _visitRepository.GetVisitCountByPatientAndDateRange(patientId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return 0;
        }
    }
    public async Task<List<Visit>> GetRecentVisitsByPatient(Guid patientId, int count) 
    {   
        try
        {
            return await _visitRepository.GetRecentVisitsByPatient(patientId, count);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetRecentVisitsByMedic(Guid medicId, int count)
    {
        try
        {
            return await _visitRepository.GetRecentVisitsByMedic(medicId, count);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsWithDiagnosis()
    {
         
        try
        {
            return await _visitRepository.GetVisitsWithDiagnosis();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsWithTreatment() 
    {   
        try
        {
            return await _visitRepository.GetVisitsWithTreatment();
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByReason(string reason)
    {
        try
        {
            return await _visitRepository.GetVisitsByReason(reason);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByDiagnosis(string diagnosis)
    {
        try
        {
            return await _visitRepository.GetVisitsByDiagnosis(diagnosis);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByTreatment(string treatment)
    {
        try
        {
            return await _visitRepository.GetVisitsByTreatment(treatment);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByMedicPatientAndDate(Guid medicId, Guid patientId, DateTime date)
    {
        try
        {
            return await _visitRepository.GetVisitsByMedicPatientAndDate(medicId, patientId, date);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
    public async Task<List<Visit>> GetVisitsByMedicPatientAndDateRange(Guid medicId, Guid patientId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _visitRepository.GetVisitsByMedicPatientAndDateRange(medicId, patientId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
}

public interface IGetVisitService
{
    Task<List<Visit>> GetVisitsByMedicAndDate(Guid medicId, DateTime date);
    Task<List<Visit>> GetVisitsByPatient(Guid patientId);
    Task<List<Visit>> GetAllVisits();
    Task<List<Visit>> GetVisitsByDate(DateTime date);
    Task<List<Visit>> GetVisitsByMedic(Guid medicId);
    Task<List<Visit>> GetVisitsByDateRange(DateTime startDate, DateTime endDate);
    Task<List<Visit>> GetVisitsByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate);
    Task<List<Visit>> GetVisitsByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate);
    Task<int> GetVisitCountByMedicAndDate(Guid medicId, DateTime date);
    Task<int> GetVisitCountByPatient(Guid patientId);
    Task<int> GetTotalVisitCount();
    Task<int> GetVisitCountByDate(DateTime date);
    Task<int> GetVisitCountByMedic(Guid medicId);
    Task<int> GetVisitCountByDateRange(DateTime startDate, DateTime endDate);
    Task<int> GetVisitCountByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate);
    Task<int> GetVisitCountByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate);
    Task<List<Visit>> GetRecentVisitsByPatient(Guid patientId, int count);
    Task<List<Visit>> GetRecentVisitsByMedic(Guid medicId, int count);
    Task<List<Visit>> GetVisitsWithDiagnosis();
    Task<List<Visit>> GetVisitsWithTreatment();
    Task<List<Visit>> GetVisitsByReason(string reason);
    Task<List<Visit>> GetVisitsByDiagnosis(string diagnosis);
    Task<List<Visit>> GetVisitsByTreatment(string treatment);
    Task<List<Visit>> GetVisitsByMedicPatientAndDate(Guid medicId, Guid patientId, DateTime date);
    Task<List<Visit>> GetVisitsByMedicPatientAndDateRange(Guid medicId, Guid patientId, DateTime startDate, DateTime endDate);
}