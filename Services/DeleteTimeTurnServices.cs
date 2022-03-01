using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class DeleteTimeTurnServices : IDeleteTimeTurnServices
    {
        private readonly ApplicationDbContext _context;

        public DeleteTimeTurnServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Delete(TimeTurnViewModel timeTurn)
        {
            try
            {
                _context.TimeTurns.Remove(timeTurn);
                await _context.SaveChangesAsync();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Tiempo {timeTurn.Id} eliminado");
                }
                else
                {
                    //File.WriteAllText("C:\\infoLog.txt", $"Tiempo {timeTurn.Id} eliminado");
                }
            }
            catch (Exception ex)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                }
                else
                {
                    //File.WriteAllText("C:\\infoLog.txt", ex.Message);
                }
                
            }
        }
    }
}
