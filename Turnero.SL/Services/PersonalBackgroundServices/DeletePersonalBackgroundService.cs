using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnero.SL.Services.PersonalBackgroundServices;

public class DeletePersonalBackgroundService(LoggerService logger, IPersonalBackgroundRepository repository) : IDeletePersonalBackgroundService
{
    public async Task DeletePersonalBackground(PersonalBackground data)
    {
        try
        {
            repository.Delete(data);
        }
        catch (Exception ex)
        {
            logger.Log($"Error in {nameof(DeletePersonalBackground)}: {ex.Message}");
            throw new Exception("An error occurred while deleting the personal background.");
        }
    }
}
public interface IDeletePersonalBackgroundService
{
    Task DeletePersonalBackground(PersonalBackground data);
}