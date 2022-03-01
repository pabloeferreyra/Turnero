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
        private readonly ILoggerServices _logger;
        public DeleteTimeTurnServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Delete(TimeTurnViewModel timeTurn)
        {
            try
            {
                _context.TimeTurns.Remove(timeTurn);
                await _context.SaveChangesAsync();
                _logger.Info($"Tiempo {timeTurn.Id} eliminado correctamente");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
