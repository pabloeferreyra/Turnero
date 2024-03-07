using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Services.Repositories;

public class TurnsRepository : RepositoryBase<Turn>, ITurnRepository
{
    private readonly IMapper mapper;

    public TurnsRepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : base(context, mapper, cache)
    {
        this.mapper = mapper;
    }



    public void Access(Turn turn)
    {
        turn.Accessed = true;
        turn.DateTurn = turn.DateTurn.ToLocalTime();
        this.Update(turn);
    }

    public async Task<Turn> GetById(Guid id)
    {
        return await this.FindByCondition(m => m.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task<TurnDTO> GetDTOById(Guid id)
    {
        return await this.FindByCondition(m => m.Id == id).ProjectTo<TurnDTO>(this.mapper.ConfigurationProvider).FirstOrDefaultAsync();
    }

    public List<Turn> GetList(DateTime? date, Guid? id)
    {
        object[] param;
        var formattedDate = new StringBuilder();
        if (date != null)
        {
            var dat = $"'{date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'";
            formattedDate.Append(dat);
        }
        else
        {
            var dat = $"'{DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'";
            formattedDate.Append(dat);
        }
        if (id != null)
        {
            param = new object[2];
            param[0] = formattedDate.ToString();
            param[1] = id != null ? id : null;
        }
        else
        {
            param = new object[]
            {
                formattedDate.ToString()
            };
        }

        return this.CallStoredProcedure("GetTurns", param);
    }

    public async Task<List<Turn>> ForExport(DateTime date, Guid id)
    {

        return await this.FindByCondition(m => m.MedicId == id && m.DateTurn.Date == date.Date)
            .Include(m => m.Medic).Include(t => t.Time)
            .OrderBy(t => t.Time.Time)
            .ToListAsync();
    }

    public bool TurnExists(Guid id)
    {
        return this.FindByCondition(m => m.Id == id).Any();
    }
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return this.FindByCondition(e => e.MedicId == medicId && e.DateTurn.Date == date && e.TimeId == timeTurn).Any();
    }

    public void DeleteTurn(Turn turn)
    {
        this.DeleteAsync(turn);
    }

    public void UpdateTurn(Turn turn)
    {
        turn.DateTurn = turn.DateTurn.ToUniversalTime();
        this.Update(turn);
    }

    public async Task CreateTurn(Turn turn)
    {
        turn.Medic = null;
        turn.Time = null;
        turn.DateTurn = turn.DateTurn.ToUniversalTime();
        await this.CreateAsync(turn);
    }
}
