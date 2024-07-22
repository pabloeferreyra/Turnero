
namespace Turnero.Services.Repositories;

public class AvailableRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Available>(context, cache), IAvailableRepository
{
    public List<Available> GetAvailables() => [.. this.FindAll()];

    public Available GetById(Guid id) => this.FindByCondition(m => m.Id == id).FirstOrDefault();

    public List<Available> GetByMedic(Guid id) => [.. this.FindByCondition(m => m.MedicId == id)];

    public void Insert(Available available) => this.Create(available);
    public async Task InsertAsync(Available available) => await this.CreateAsync(available);

    public void Edit(Available available) => this.Update(available);
    public async Task EditAsync(Available available) => await this.UpdateAsync(available);
}
