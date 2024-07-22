namespace Turnero.Services;

public class InsertAvailableService(IAvailableRepository repository) : IInsertAvailableService
{
    private readonly IAvailableRepository _repository = repository;
    public void Insert(Available available) => _repository.Insert(available);
    public async Task InsertAsync(Available available) => await _repository.InsertAsync(available);
}
