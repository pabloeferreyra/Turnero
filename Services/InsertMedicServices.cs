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

        public InsertMedicServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Create(Medic medic)
        {
            try
            {
                medic.Id = Guid.NewGuid();
                _context.Add(medic);
                await _context.SaveChangesAsync();
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Creado Medico {medic.Id}");
            }
            catch (Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }
    }
}
