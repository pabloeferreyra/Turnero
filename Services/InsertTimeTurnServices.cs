using System;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class InsertTimeTurnServices : IInsertTimeTurnServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerServices _logger;

        public InsertTimeTurnServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Create(TimeTurnViewModel timeTurnViewModel)
        {
            try
            {
                timeTurnViewModel.Id = Guid.NewGuid();
                _context.Add(timeTurnViewModel);
                _logger.Debug($"Horario {timeTurnViewModel.Id} creado");
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
