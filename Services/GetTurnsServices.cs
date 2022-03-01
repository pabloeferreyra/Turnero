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

namespace Turnero.Services
{
    public class GetTurnsServices : IGetTurnsServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerServices _logger;

        public GetTurnsServices(ApplicationDbContext context, ILoggerServices logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Turn>> GetTurns(DateTime? dateTurn, Guid? medicId)
        {
            try
            {
                List<Turn> turns = new List<Turn>();
                if (medicId != null)
                {
                    if (dateTurn.HasValue)
                        turns = await _context.Turns.Include(m => m.Medic).Include(t => t.Time).Where(m => m.Medic.Id == medicId && m.DateTurn == dateTurn).OrderBy(t => t.Time.Time).ToListAsync();
                    else
                        turns = await _context.Turns.Include(m => m.Medic).Include(t => t.Time).Where(m => m.Medic.Id == medicId && m.DateTurn == DateTime.Today).OrderBy(t => t.Time.Time).ToListAsync();
                }
                else
                {
                    if (dateTurn.HasValue)
                        turns = await _context.Turns.Include(m => m.Medic).Include(t => t.Time).Where(m => m.DateTurn == dateTurn).OrderBy(t => t.Time.Time).ToListAsync();
                    else
                        turns = await _context.Turns.Include(m => m.Medic).Include(t => t.Time).Where(m => m.DateTurn <= DateTime.Today && m.DateTurn >= DateTime.Today.AddDays(-10)).OrderBy(t => t.Time.Time).ToListAsync();
                }

                _logger.Info($"{dateTurn} Turnos {turns.Count()} llegaron correctamente");
                return turns;
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
                return await _context.Turns.Include(m => m.Medic).Include(t => t.Time)
                    .FirstOrDefaultAsync(m => m.Id == id);
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
                return _context.Turns.Any(e => e.Id == id);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
