using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Services.Repositories
{
    public class TimeTurnRepository : RepositoryBase<TimeTurnViewModel>, ITimeTurnRepository
    {
        public TimeTurnRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<TimeTurnViewModel>> GetList() {
            return await FindAll().OrderBy(t => t.Time).ToListAsync();
        }

        public IQueryable<TimeTurnViewModel> GetQueryable()
        {
            return FindAll().OrderBy(t => t.Time);
        }

        public async Task<TimeTurnViewModel> GetbyId(Guid id)
        {
            return await FindByCondition(t => t.Id == id).FirstOrDefaultAsync();
        }

        public bool Exists(Guid id)
        {
            return FindByCondition(t => t.Id == id).Any();
        }

        public async Task CreateTT(TimeTurnViewModel timeTurn)
        {
            await CreateAsync(timeTurn);
        }

        public void DeleteTT(TimeTurnViewModel timeTurn)
        {
            Delete(timeTurn);
        }
    }
}
