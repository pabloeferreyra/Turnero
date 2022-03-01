using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class GetTimeTurnsServices : IGetTimeTurnsServices
    {
        private readonly ApplicationDbContext _context;

        public GetTimeTurnsServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimeTurnViewModel>> GetTimeTurns()
        {
            try
            {
                List<TimeTurnViewModel> timeTurns;
                timeTurns = await _context.TimeTurns.OrderBy(t => t.Time).ToListAsync();
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Tiempos obtenidos");
                return timeTurns;
            }
            catch(Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return null;
            }
        }

        public IQueryable<TimeTurnViewModel> GetTimeTurnsQ()
        {
            try
            {
                IQueryable<TimeTurnViewModel> timeTurns;
                timeTurns = _context.TimeTurns.OrderBy(t => t.Time);
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Tiempos obtenidos");
                return timeTurns;
            }
            catch (Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return null;
            }
        }

        public async Task<TimeTurnViewModel> GetTimeTurn(Guid id)
        {
            try
            {
                TimeTurnViewModel timeTurn;
                timeTurn = await _context.TimeTurns.FirstOrDefaultAsync(t => t.Id == id);
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Tiempo {id} obtenido");
                return timeTurn;
            }
            catch(Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return null;
            }
        }

        public async Task<bool> TimeTurnViewModelExists(Guid id)
        {
            try
            {
                return _context.TimeTurns.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return false;
            }
        }
    }
}
