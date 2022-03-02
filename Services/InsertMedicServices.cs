using System;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class InsertMedicServices : IInsertMedicServices
    {
        private readonly ILoggerServices _logger;
        private readonly IMedicRepository _medicRepository;

        public InsertMedicServices(ILoggerServices logger, IMedicRepository medicRepository)
        {
            _logger = logger;
            _medicRepository = medicRepository;
        }

        public async Task Create(Medic medic)
        {
            try
            {
                await _medicRepository.NewMedic(medic);
                _logger.Info($"Creado Medico {medic.Id}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
