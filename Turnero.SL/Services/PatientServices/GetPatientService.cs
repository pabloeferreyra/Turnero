namespace Turnero.SL.Services.PatientServices;

public class GetPatientService(LoggerService logger, IPatientRepository patientRepository) : IGetPatientService
{
    private readonly LoggerService _logger = logger;
    private readonly IPatientRepository _patientRepository = patientRepository;
    public async Task<List<PatientDTO>> GetPatients()
    {
        try
        {
            return await _patientRepository.GetList();
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(GetPatients)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving patients.");
        }
    }

    public IQueryable<PatientDTO> GetAllPatients()
    {
        try
        {
            return _patientRepository.GetAll();
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(GetAllPatients)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving all patients.");
        }
    }

    public async Task<Patient> GetPatientById(Guid id)
    {
        try
        {
            return await _patientRepository.GetById(id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Log($"Patient not found in {nameof(GetPatientById)}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(GetPatientById)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the patient.");
        }
    }
    public async Task<IQueryable<PatientDTO>> SearchPatients(string search)
    {
        try
        {
            return await _patientRepository.SearchByNameOrDni(search);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(SearchPatients)}: {ex.Message}");
            throw new Exception("An error occurred while searching for patients.");
        }
    }
}

public interface IGetPatientService
{
    Task<List<PatientDTO>> GetPatients();
    IQueryable<PatientDTO> GetAllPatients();
    Task<Patient> GetPatientById(Guid id);
    Task<IQueryable<PatientDTO>> SearchPatients(string search);
}
