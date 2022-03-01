using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IGetTurnsServices
    {
        public Task<List<Turn>> GetTurns(ClaimsPrincipal currentUser, DateTime? dateTurn, Guid? medicId);

        public Task<Turn> GetTurn(Guid id);

        public Task<bool> Exists(Guid id);
    }
}