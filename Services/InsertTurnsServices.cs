using System;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class InsertTurnsServices : IInsertTurnsServices
    {
        private readonly ApplicationDbContext _context;
        public InsertTurnsServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateTurnAsync( Turn turn)
        {
            try
            {
                turn.Id = Guid.NewGuid();
                _context.Add(turn);
                await _context.SaveChangesAsync();
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Turno agregado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return false;
            }
        }
    }
}
