using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class UpdateTurnsServices : IUpdateTurnsServices
    {
        private readonly ILoggerServices _logger;
        private readonly ITurnRepository _turnRepository;

        public UpdateTurnsServices(ILoggerServices logger, ITurnRepository turnRepository)
        {
            _logger = logger;
            _turnRepository = turnRepository;
        }

        public void Accessed(Turn turn)
        {
            try
            {
                if(turn.DateTurn <= DateTime.Today)
                {
                    _turnRepository.Access(turn);
                    _ = Task.Run(() =>
                    {
                        _logger.Debug($"Turno {turn.Id} ingresado");
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public async void Update(Turn turn)
        {
            try
            {
                await _turnRepository.UpdateTurn(turn);
                _ = Task.Run(() =>
                  {
                      _logger.Debug($"Turno {turn.Id} Actualizado");
                  });
                
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public async void Delete(Turn turn)
        {
            try
            {
                await _turnRepository.DeleteTurn(turn);
                _ = Task.Run(() =>
                {
                    _logger.Debug($"Turno {turn.Id} Eliminado");
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
