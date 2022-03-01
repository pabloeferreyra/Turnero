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

        private GetTurnsServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Turn>> GetTurns(ClaimsPrincipal currentUser, DateTime? dateTurn, Guid? medicId)
        {
            try
            {
                var user = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                var medic = await _context.Medics.Where(m => m.UserGuid == user).FirstOrDefaultAsync();
                List<Turn> turns = new List<Turn>();
                if (medicId != null)
                {
                    if (dateTurn.HasValue)
                        turns = await _context.Turns.Where(m => m.MedicId == medicId && m.DateTurn == dateTurn).OrderBy(t => t.Time.Time).ToListAsync();
                    else
                        turns = await _context.Turns.Where(m => m.MedicId == medicId && m.DateTurn == DateTime.Today).OrderBy(t => t.Time.Time).ToListAsync();
                }
                else
                {
                    if (dateTurn.HasValue)
                        turns = await _context.Turns.Where(m => m.DateTurn == dateTurn).OrderBy(t => t.Time.Time).ToListAsync();
                    else
                        turns = await _context.Turns.OrderBy(t => t.Time.Time).ToListAsync();
                }
                List<Turn> turns1 = new List<Turn>();
                foreach (var t in turns)
                {
                    t.Time = await _context.TimeTurns.FirstOrDefaultAsync(ti => ti.Id == t.TimeId);
                    t.Medic = await _context.Medics.FirstOrDefaultAsync(m => m.Id == t.MedicId);
                    turns1.Add(t);
                }
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"{dateTurn} Turnos {turns1.Count()} llegaron correctamente");
                return turns1;
            }
            catch (Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return null;
            }
        }

        public async Task<Turn> GetTurn(Guid id)
        {
            try
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Turno {id}");
                return await _context.Turns
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch(Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return null;
            }
        }

        public async Task<bool> Exists(Guid id)
        {
            try
            {
                return _context.Turns.Any(e => e.Id == id);
            }
            catch(Exception ex)
            {
                File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
                return false;
            }
        }
    }
}
