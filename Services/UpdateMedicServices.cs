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

        private UpdateMedicServices(ApplicationDbContext context)
        {
            _context = context;
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
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
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
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }
    }
}
