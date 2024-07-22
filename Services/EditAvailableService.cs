
namespace Turnero.Services;

public class EditAvailableService(IAvailableRepository repository) : IEditAvailableService
{
    private readonly IAvailableRepository _repository = repository;
    public void Edit(Available available) => this._repository.Edit(available);
    public async Task EditAsync(Available available) => await this._repository.EditAsync(available);
}
