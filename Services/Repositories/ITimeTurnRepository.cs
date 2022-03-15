using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Repositories
{
    public interface ITimeTurnRepository
    {
        Task<List<TimeTurnViewModel>> GetList();
        IQueryable<TimeTurnViewModel> GetQueryable();
        Task<TimeTurnViewModel> GetbyId(Guid id);
        bool Exists(Guid id);
        Task CreateTT(TimeTurnViewModel timeTurn);

        void DeleteTT(TimeTurnViewModel timeTurn);
    }
}
