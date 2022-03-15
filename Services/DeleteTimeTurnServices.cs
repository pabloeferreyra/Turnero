using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class DeleteTimeTurnServices : IDeleteTimeTurnServices
    {
        private readonly ILoggerServices _logger;
        private readonly ITimeTurnRepository _timeTurnRepository;
        public DeleteTimeTurnServices(ILoggerServices logger,
                                      ITimeTurnRepository timeTurnRepository)
        {
            _logger = logger;
            _timeTurnRepository = timeTurnRepository;
        }

        public void Delete(TimeTurnViewModel timeTurn)
        {
            try
            {
                _timeTurnRepository.DeleteTT(timeTurn);
                _logger.Info($"Tiempo {timeTurn.Id} eliminado correctamente");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
