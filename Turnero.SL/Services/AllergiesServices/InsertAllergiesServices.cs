namespace Turnero.SL.Services.AllergiesServices;

public class InsertAllergiesServices(IAllergiesRepository _repository, LoggerService _loggerService) : IInsertAllergiesServices
{
    public async Task InsertAllergy(Allergies allergy)
    {
        try
        {
            await _repository.CreateAllergy(allergy);
        }
        catch (Exception ex)
        {
            _loggerService.Log($"Error in {nameof(InsertAllergy)}: {ex.Message}");
            throw new Exception("An error occurred while inserting the allergy.");
        }
    }
}
public interface IInsertAllergiesServices
{
    Task InsertAllergy(Allergies allergy);
}
