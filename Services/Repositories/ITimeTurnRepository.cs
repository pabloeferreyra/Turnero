using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Repositories;

public interface ITimeTurnRepository
{
    Task<List<TimeTurn>> GetList();
    IQueryable<TimeTurn> GetQueryable();
    Task<TimeTurn> GetbyId(Guid id);
    bool Exists(Guid id);
    Task CreateTT(TimeTurn timeTurn);

    void DeleteTT(TimeTurn timeTurn);
    
    Task<List<TimeTurn>> GetCachedTimes();
}
