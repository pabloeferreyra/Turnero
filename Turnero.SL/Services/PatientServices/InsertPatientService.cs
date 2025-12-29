namespace Turnero.SL.Services.PatientServices;

public class InsertPatientService(LoggerService logger, IPatientRepository patientRepository) : IInsertPatientService
{
    private readonly LoggerService _logger = logger;
    private readonly IPatientRepository _patientRepository = patientRepository;
    public async Task InsertPatient(Patient patient)
    {
        try
        {
            patient.Id = Guid.NewGuid();
            patient.PersonalBackground = new();
            patient.PerinatalBackground = new();
            patient.Parent = new();
            patient.CongErrors = new();
            await _patientRepository.NewPatient(patient);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(InsertPatient)}: {ex.Message}");
            throw new Exception("An error occurred while inserting the patient.");
        }
    }
}

public interface IInsertPatientService
{
    Task InsertPatient(Patient patient);
}