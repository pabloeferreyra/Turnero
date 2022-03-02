using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IGetTimeTurnsServices
    {
        Task<List<TimeTurnViewModel>> GetTimeTurns();
        IQueryable<TimeTurnViewModel> GetTimeTurnsQ();
        Task<TimeTurnViewModel> GetTimeTurn(Guid id);
        bool TimeTurnViewModelExists(Guid id);
    }
}
