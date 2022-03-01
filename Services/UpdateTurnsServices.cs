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

        public UpdateTurnsServices(ApplicationDbContext context)
        {
            _context = context;
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
                    //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Turno {turn.Id} ingresado");
                }
            }
            catch (Exception ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }

        public async void Update(Turn turn)
        {
            try
            {
                _context.Update(turn);
                await _context.SaveChangesAsync();
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Turno {turn.Id} Actualizado");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }

        public async void Delete(Turn turn)
        {
            try
            {
                _context.Turns.Remove(turn);
                await _context.SaveChangesAsync();
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", $"Turno {turn.Id} Eliminado");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //File.WriteAllText("@/tmp/TurneroLogs/infoLog.txt", ex.Message);
            }
        }
    }
}
