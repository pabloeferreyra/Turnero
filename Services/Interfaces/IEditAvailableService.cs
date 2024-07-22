namespace Turnero.Services.Interfaces;

public interface IEditAvailableService
{
    void Edit(Available available);
    Task EditAsync(Available available);
}
