using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using ApplicationDbContext = Turnero.Data.ApplicationDbContext;

namespace Turnero.Services
{
    class InsertTurnsServices : IInsertTurnsServices
    {
        private readonly ApplicationDbContext _context;
        private InsertTurnsServices(ApplicationDbContext context)
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
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Turno agregado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return false;
            }
        }
    }
}
