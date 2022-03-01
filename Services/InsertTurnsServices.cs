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
        private readonly ILoggerServices _logger;
        public InsertTurnsServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<bool> CreateTurnAsync( Turn turn)
        {
            try
            {
                turn.Id = Guid.NewGuid();
                _context.Add(turn);
                await _context.SaveChangesAsync();
                _logger.Debug("Turno agregado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
