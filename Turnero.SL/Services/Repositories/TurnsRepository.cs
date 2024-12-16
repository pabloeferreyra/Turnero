using AutoMapper.QueryableExtensions;
using System.Globalization;

namespace Turnero.SL.Services.Repositories;

public class TurnsRepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : RepositoryBase<Turn>(context, mapper, cache), ITurnRepository
{
    private readonly IMapper mapper = mapper;

    public void Access(Turn turn)
    {
        turn.Accessed = true;
        turn.DateTurn = turn.DateTurn.ToUniversalTime();
        Update(turn);
    }

    public async Task<Turn> GetById(Guid id)
    {
        return await FindByCondition(m => m.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task<TurnDTO> GetDTOById(Guid id)
    {
        return await FindByCondition(m => m.Id == id).ProjectTo<TurnDTO>(mapper.ConfigurationProvider).FirstOrDefaultAsync();
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

        return CallStoredProcedure("GetTurns", param);
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
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return FindByCondition(e => e.MedicId == medicId && e.DateTurn.Date == date && e.TimeId == timeTurn).Any();
    }

    public void DeleteTurn(Turn turn)
    {
        DeleteAsync(turn);
    }

    public void UpdateTurn(Turn turn)
    {
        turn.DateTurn = turn.DateTurn.ToUniversalTime();
        Update(turn);
    }

    public async Task CreateTurn(Turn turn)
    {
        turn.Medic = null;
        turn.Time = null;
        turn.DateTurn = turn.DateTurn.ToUniversalTime();
        await CreateAsync(turn);
    }
}
