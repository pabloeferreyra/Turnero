namespace Turnero.Services.Repositories;

public interface IAvailableRepository
{
    List<Available> GetAvailables();
    Available GetById(Guid id);
    List<Available> GetByMedic(Guid id);
    void Insert(Available available);
    Task InsertAsync(Available available);
    void Edit(Available available);
    Task EditAsync(Available available);
}
