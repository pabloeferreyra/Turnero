using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class GetTurnsServices : IGetTurnsServices
    {
        private readonly ILoggerServices _logger;
        private readonly ITurnRepository _turnRepository;

        public GetTurnsServices(ILoggerServices logger, ITurnRepository turnRepository)
        {
            _logger = logger;
            _turnRepository = turnRepository;
        }
        public async Task<List<Turn>> GetTurns(DateTime? dateTurn, Guid? medicId)
        {
            try
            {
                _logger.Info($"{dateTurn} Turnos llegaron correctamente");
                return await _turnRepository.GetList(dateTurn, medicId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public async Task<Turn> GetTurn(Guid id)
        {
            try
            {
                _logger.Info($"Turno {id}");
                return await _turnRepository.GetById(id);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;
            }
        }

        public bool Exists(Guid id)
        {
            try
            {
                return _turnRepository.TurnExists(id);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
