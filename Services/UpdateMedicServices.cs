using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class UpdateMedicServices : IUpdateMedicServices
    {
        private readonly ILoggerServices _logger;
        private readonly IMedicRepository _medicRepository;

        public UpdateMedicServices(ILoggerServices logger, IMedicRepository medicRepository)
        {
            _logger = logger;
            _medicRepository = medicRepository;
        }

        public async Task<bool> Update(Medic medic)
        {
            try
            {
                await _medicRepository.UpdateMedic(medic);
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
                await _medicRepository.DeleteMedic(medic);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
