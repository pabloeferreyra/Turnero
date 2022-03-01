using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class UpdateMedicServices : IUpdateMedicServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerServices _logger;

        public UpdateMedicServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Update(Medic medic)
        {
            try
            {
                _context.Update(medic);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        public async Task Delete(Medic medic)
        {
            try
            {
                _context.Medics.Remove(medic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
