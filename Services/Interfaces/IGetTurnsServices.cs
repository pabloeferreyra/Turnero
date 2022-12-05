using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces;

public interface IGetTurnsServices
{
    public Task<List<Turn>> GetTurns(DateTime? dateTurn, Guid? medicId);

    public Task<Turn> GetTurn(Guid id);

    public bool Exists(Guid id);

    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn);
}