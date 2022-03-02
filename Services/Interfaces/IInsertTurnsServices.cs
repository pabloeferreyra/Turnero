using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IInsertTurnsServices
    {
        public Task<bool> CreateTurnAsync(Turn turn);
    }
}
