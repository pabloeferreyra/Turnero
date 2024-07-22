namespace Turnero.Services.Interfaces;

public interface IInsertAvailableService
{
    void Insert(Available available);
    Task InsertAsync(Available available);
}
