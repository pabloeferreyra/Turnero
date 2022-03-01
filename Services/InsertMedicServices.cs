using System;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class InsertMedicServices : IInsertMedicServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerServices _logger;

        public InsertMedicServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Create(Medic medic)
        {
            try
            {
                medic.Id = Guid.NewGuid();
                _context.Add(medic);
                await _context.SaveChangesAsync();
                _logger.Info($"Creado Medico {medic.Id}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
