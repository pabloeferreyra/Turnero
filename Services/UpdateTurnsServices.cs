using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class UpdateTurnsServices : IUpdateTurnsServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerServices _logger;

        public UpdateTurnsServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }

        public async void Accessed(ClaimsPrincipal currentUser, Turn turn)
        {
            try
            {
                if(turn.DateTurn <= DateTime.Today)
                {
                    turn.Accessed = true;
                    _context.Update(turn);
                    await _context.SaveChangesAsync();
                    _logger.Debug($"Turno {turn.Id} ingresado");
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
                _context.Update(turn);
                await _context.SaveChangesAsync();
                _logger.Debug($"Turno {turn.Id} Actualizado");
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
                _context.Turns.Remove(turn);
                await _context.SaveChangesAsync();
                _logger.Debug($"Turno {turn.Id} Eliminado");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
    }
}
