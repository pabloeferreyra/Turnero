using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Repositories
{
    public interface ITurnRepository
    {
        Task<List<Turn>> GetList(DateTime? date, Guid? id);
        Task<Turn> GetById(Guid id);
        bool TurnExists(Guid id);
        Task Access(Turn turn);
        Task DeleteTurn(Turn turn);
        Task UpdateTurn(Turn turn);
        Task CreateTurn(Turn turn);
    }
}
