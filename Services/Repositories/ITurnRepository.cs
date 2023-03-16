using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Repositories;

public interface ITurnRepository
{
    IQueryable<TurnDTO> GetListDto();
    Task<List<Turn>> GetList(DateTime? date, Guid? id);
    Task<Turn> GetById(Guid id);
    Task<TurnDTO> GetDTOById(Guid id);
    bool TurnExists(Guid id);
    bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn);
    void Access(Turn turn);
    Task DeleteTurn(Turn turn);
    Task UpdateTurn(Turn turn);
    Task CreateTurn(Turn turn);
}
