using Microsoft.EntityFrameworkCore;
using TurneroAPI.Data;
using TurneroAPI.Models;

namespace TurneroAPI.Services.Repositories
{
    public class MedicRepository : RepositoryBase<Medic>, IMedicRepository
    {
        public MedicRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<Medic>> GetList()
        {
            return await FindAll().ToListAsync();
        }

        public async Task<Medic> GetById(Guid id)
        {
            return await FindByCondition(m => m.Id == id).SingleOrDefaultAsync();
        }

        public async Task<Medic> GetByUserId(string id)
        {
            return await FindByCondition(m => m.UserGuid == id).SingleOrDefaultAsync();
        }

        public bool Exists(Guid id)
        {
            return FindByCondition(m => m.Id == id).Any();
        }

        public async Task NewMedic(Medic medic)
        {
            await CreateAsync(medic);
        }

        public async Task DeleteMedic(Medic medic)
        {
            Delete(medic);
        }

        public async Task UpdateMedic(Medic medic)
        {
            await UpdateAsync(medic);
        }
    }
}
