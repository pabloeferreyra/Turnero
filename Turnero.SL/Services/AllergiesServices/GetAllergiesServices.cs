namespace Turnero.SL.Services.AllergiesServices;

public class GetAllergiesServices (IAllergiesRepository _allergiesRepository, LoggerService _logger) : IGetAllergiesServices
{
    public async Task<List<Allergies>> GetAllergiesByPatient(Guid? id)
    {
        try
        {
            return await _allergiesRepository.GetAllergiesByPatient(id);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(GetAllergiesByPatient)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the patient's allergies.");
        }
    }

    public async Task<IQueryable<Allergies>> GetAllergies(Guid id)
    {
        try
        {
            return await _allergiesRepository.SearchAllergies(id);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(GetAllergies)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the patient's allergies query.");
        }
    }

    public async Task<Allergies?> Get(Guid? id)
    {
        try
        {
            return await _allergiesRepository.Get(id);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(Get)}: {ex.Message}");
            throw new Exception("An error occurred while retrieving the allergy details.");
        }
    }
}

public interface IGetAllergiesServices
{
    Task<List<Allergies>> GetAllergiesByPatient(Guid? id);
    Task<IQueryable<Allergies>> GetAllergies(Guid id);
    Task<Allergies?> Get(Guid? id);
}

