using System;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class DeleteTimeTurnServices : IDeleteTimeTurnServices
    {
        private readonly ApplicationDbContext _context;

        private DeleteTimeTurnServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Delete(TimeTurnViewModel timeTurn)
        {
            try
            {
                _context.TimeTurns.Remove(timeTurn);
                await _context.SaveChangesAsync();
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Tiempo {timeTurn.Id} eliminado");
            }
            catch (Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }
    }
}
