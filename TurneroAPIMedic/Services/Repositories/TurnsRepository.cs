using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;

namespace TurneroAPI.Services.Repositories
{
    public class TurnsRepository : RepositoryBase<Turn>, ITurnRepository
    {
        public TurnsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task Access(Turn turn)
        {
            turn.Accessed = true;
            await UpdateAsync(turn);
        }

        public async Task<Turn> GetById(Guid id)
        {
            return await FindByCondition(m => m.Id == id)
                .Include(m => m.Medic)
                .Include(t => t.Time).SingleOrDefaultAsync();
        }

        public async Task<List<Turn>> GetList(DateTime? date, Guid? id)
        {
            if (id != null)
            {
                if (date != null)
                {
                    return await FindByCondition(m => m.MedicId == id && m.DateTurn.Date == date.Value.Date)
                        .Include(m => m.Medic).Include(t => t.Time)
                        .OrderBy(t => t.Time.Time)
                        .ToListAsync();
                }
                else
                {
                    return await FindByCondition(m => m.MedicId == id && m.DateTurn == DateTime.Today)
                        .Include(m => m.Medic).Include(t => t.Time)
                        .OrderBy(t => t.Time.Time).ToListAsync();
                }
            }
            else
            {
                if (date != null)
                {
                    return await FindByCondition(m => m.DateTurn == date)
                        .Include(m => m.Medic).Include(t => t.Time)
                        .OrderBy(t => t.Time.Time).ToListAsync();
                }
                else
                {
                    return await FindByCondition(m => m.DateTurn == DateTime.Today)
                        .Include(m => m.Medic).Include(t => t.Time)
                        .OrderBy(t => t.Time.Time).ToListAsync();
                }
            }
        }
        public async Task<List<Turn>> ForExport(DateTime date, Guid id)
        {

            return await FindByCondition(m => m.MedicId == id && m.DateTurn.Date == date.Date)
                .Include(m => m.Medic).Include(t => t.Time)
                .OrderBy(t => t.Time.Time)
                .ToListAsync();
        }

        public bool TurnExists(Guid id)
        {
            return FindByCondition(m => m.Id == id).Any();
        }

        public async Task DeleteTurn(Turn turn)
        {
            turn.MedicId = turn.Medic.Id;
            turn.TimeId = turn.Time.Id;
            turn.Medic = null;
            turn.Time = null;
            Delete(turn);
        }

        public async Task UpdateTurn(Turn turn)
        {
            turn.MedicId = turn.Medic.Id;
            turn.TimeId = turn.Time.Id;
            turn.Medic = null;
            turn.Time = null;
            await UpdateAsync(turn);
        }

        public async Task CreateTurn(Turn turn)
        {
            turn.MedicId = turn.Medic.Id;
            turn.TimeId = turn.Time.Id;
            turn.Medic = null;
            turn.Time = null;
            await CreateAsync(turn);
        }
    }
}
