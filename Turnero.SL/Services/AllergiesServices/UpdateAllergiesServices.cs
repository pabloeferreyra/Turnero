namespace Turnero.SL.Services.AllergiesServices;

public class UpdateAllergiesServices(IAllergiesRepository _allergiesRepository, LoggerService _logger) : IUpdateAllergiesServices
{
    public async Task UpdateAllergy(Allergies allergy)
    {
        try
        {
            await _allergiesRepository.UpdateAllergy(allergy);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in {nameof(UpdateAllergy)}: {ex.Message}");
            throw new Exception("An error occurred while updating the allergy.");
        }
    }
}
public interface IUpdateAllergiesServices
{
    Task UpdateAllergy(Allergies allergy);
}
