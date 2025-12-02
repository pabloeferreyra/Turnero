namespace Turnero.SL.Services.AllergiesServices;

public class DeleteAllergiesServices(IAllergiesRepository _repository, LoggerService _loggerService) : IDeleteAllergiesServices
{
    public void DeleteAllergy(Allergies allergy)
    {
        try
        {
            _repository.DeleteAllergy(allergy);
        }
        catch (Exception ex)
        {
            _loggerService.Log($"Error in {nameof(DeleteAllergy)}: {ex.Message}");
            throw new Exception("An error occurred while deleting the allergy.");
        }
    }
}

public interface IDeleteAllergiesServices
{
    void DeleteAllergy(Allergies allergy);
}
