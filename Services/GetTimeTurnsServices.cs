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
        private readonly ILoggerServices _logger;

        public GetTimeTurnsServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TimeTurnViewModel>> GetTimeTurns()
        {
            try
            {
                List<TimeTurnViewModel> timeTurns;
                timeTurns = await _context.TimeTurns.OrderBy(t => t.Time).ToListAsync();
                _logger.Debug("Tiempos obtenidos");
                return timeTurns;
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public IQueryable<TimeTurnViewModel> GetTimeTurnsQ()
        {
            try
            {
                IQueryable<TimeTurnViewModel> timeTurns;
                timeTurns = _context.TimeTurns.OrderBy(t => t.Time);
                _logger.Debug("Tiempos obtenidos");
                return timeTurns;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public async Task<TimeTurnViewModel> GetTimeTurn(Guid id)
        {
            try
            {
                TimeTurnViewModel timeTurn;
                timeTurn = await _context.TimeTurns.FirstOrDefaultAsync(t => t.Id == id);
                _logger.Info($"Tiempo {id} obtenido");
                return timeTurn;
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public bool TimeTurnViewModelExists(Guid id)
        {
            try
            {
                return _context.TimeTurns.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
