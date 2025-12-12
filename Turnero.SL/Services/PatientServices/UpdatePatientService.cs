namespace Turnero.SL.Services.PatientServices;

public class UpdatePatientService(LoggerService logger, IPatientRepository patientRepository) : IUpdatePatientService
{
    private readonly LoggerService _logger = logger;
    private readonly IPatientRepository _patientRepository = patientRepository;
    public async Task UpdatePatient(Patient patient)
    {
        try
        {
            await _patientRepository.UpdatePatient(patient);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(UpdatePatient)}: {ex.Message}");
            throw new Exception("An error occurred while updating the patient.");
        }
    }

    public async Task DeletePatient(Guid patientId)
    {
        try
        {
            var patient = await _patientRepository.GetById(patientId) ?? throw new InvalidOperationException("Patient not found.");
            _patientRepository.DeletePatient(patient);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(DeletePatient)}: {ex.Message}");
            throw new Exception("An error occurred while deleting the patient.");
        }
    }
}

public interface IUpdatePatientService
{
    Task UpdatePatient(Patient patient);
    Task DeletePatient(Guid patientId);
}