using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Services.Repositories;

public class TimeTurnRepository : RepositoryBase<TimeTurn>, ITimeTurnRepository
{
    public TimeTurnRepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : base(context, mapper, cache)
    {

    }

    public async Task<List<TimeTurn>> GetList() {
        return await FindAll().OrderBy(t => t.Time).ToListAsync();
    }

    public IQueryable<TimeTurn> GetQueryable()
    {
        return FindAll().OrderBy(t => t.Time);
    }

    public async Task<TimeTurn> GetbyId(Guid id)
    {
        return await FindByCondition(t => t.Id == id).FirstOrDefaultAsync();
    }

    public bool Exists(Guid id)
    {
        return FindByCondition(t => t.Id == id).Any();
    }

    public async Task CreateTT(TimeTurn timeTurn)
    {
        await CreateAsync(timeTurn);
    }

    public void DeleteTT(TimeTurn timeTurn)
    {
        Delete(timeTurn);
    }

    public async Task<List<TimeTurn>> GetCachedTimes()
    {
        return await GetCachedData("timeTurns", GetList);
    }
}
