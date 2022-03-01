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
        private readonly ILoggerServices _logger;

        public GetMedicsServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Medic>> GetMedics()
        {
            try
            {
                var med = await _context.Medics.ToListAsync();
                _logger.Debug("Medicos traidos correctamente");
                return med;
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return new List<Medic>();
            }
        }

        public async Task<Medic> GetMedicById(Guid id)
        {
            try
            {
                Medic med = await _context.Medics.SingleOrDefaultAsync(m => m.Id == id);
                _logger.Debug("Medico traido correctamente");
                return med;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return new Medic();
            }
        }

        public async Task<Medic> GetMedicByUserId(string id)
        {
            try
            {
                Medic med = await _context.Medics.SingleOrDefaultAsync(m => m.UserGuid == id);
                _logger.Debug("Medico traido correctamente por usuario");
                return med;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
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
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
