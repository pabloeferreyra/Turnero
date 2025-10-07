namespace Turnero.SL.Services.Repositories;

public class HistoryRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<History>(context, cache), IHistoryRepository
{
    public async Task<History?> GetByPatientId(Guid patientId)
    {
        return await FindByCondition(h => h.PatientId == patientId).SingleOrDefaultAsync();
    }
    public async Task<History?> GetById(Guid historyId)
    {
        return await FindByCondition(h => h.Id == historyId).SingleOrDefaultAsync();
    }
    public async Task CreateHistory(History history)
    {
        await CreateAsync(history);
    }
    public async Task UpdateHistory(History history)
    {
        await UpdateAsync(history);
    }
    public void DeleteHistory(History history)
    {
        DeleteAsync(history);
    }
}

public interface IHistoryRepository
{
    Task<History?> GetByPatientId(Guid patientId);
    Task<History?> GetById(Guid historyId);
    Task CreateHistory(History history);
    Task UpdateHistory(History history);
    void DeleteHistory(History history);
}
