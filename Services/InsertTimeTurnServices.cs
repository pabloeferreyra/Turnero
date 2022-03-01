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

        private InsertTimeTurnServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Create(TimeTurnViewModel timeTurnViewModel)
        {
            try
            {
                timeTurnViewModel.Id = Guid.NewGuid();
                _context.Add(timeTurnViewModel);
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Horario {timeTurnViewModel.Id} creado");
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }
    }
}
