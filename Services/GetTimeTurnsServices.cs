using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class GetTimeTurnsServices : IGetTimeTurnsServices
    {
        private readonly ILoggerServices _logger;
        private readonly ITimeTurnRepository _timeTurnRepository;

        public GetTimeTurnsServices(ILoggerServices logger,
                                    ITimeTurnRepository timeTurnRepository)
        {
            _logger = logger;
            _timeTurnRepository = timeTurnRepository;
        }

        public async Task<List<TimeTurnViewModel>> GetTimeTurns()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    _logger.Debug("Tiempos obtenidos");
                });
                return await _timeTurnRepository.GetList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public IQueryable<TimeTurnViewModel> GetTimeTurnsQ()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    _logger.Debug("Tiempos obtenidos");
                });
                return _timeTurnRepository.GetQueryable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public async Task<TimeTurnViewModel> GetTimeTurn(Guid id)
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    _logger.Info($"Tiempo {id} obtenido");
                });
                return await _timeTurnRepository.GetbyId(id);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public bool TimeTurnViewModelExists(Guid id)
        {
            try
            {
                return _timeTurnRepository.Exists(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
