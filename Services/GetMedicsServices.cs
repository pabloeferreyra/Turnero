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
    public class GetMedicsServices : IGetMedicsServices
    {
        private readonly ApplicationDbContext _context;

        private GetMedicsServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medic>> GetMedics()
        {
            try
            {
                var med = await _context.Medics.ToListAsync();
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Medicos traidos correctamente");
                return med;
            }
            catch(Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return new List<Medic>();
            }
        }

        public async Task<Medic> GetMedicById(Guid id)
        {
            try
            {
                Medic med = await _context.Medics.SingleOrDefaultAsync(m => m.Id == id);
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Medico traido correctamente");
                return med;
            }
            catch (Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return new Medic();
            }
        }

        public async Task<Medic> GetMedicByUserId(string id)
        {
            try
            {
                Medic med = await _context.Medics.SingleOrDefaultAsync(m => m.UserGuid == id);
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", "Medico traido correctamente");
                return med;
            }
            catch (Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return new Medic();
            }
        }

        public bool ExistMedic(Guid id)
        {
            try
            {
                return _context.Medics.Any(m => m.Id == id);

            }
            catch(Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return false;
            }
        }
    }
}
