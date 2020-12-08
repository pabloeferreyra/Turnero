using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Turnero.Models;
using ApplicationDbContext = Turnero.Data.ApplicationDbContext;

namespace Turnero.Controllers
{
    public class TurnsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;
        public ILogger<AdministrationController> Logger { get; }

        public TurnsController(ApplicationDbContext context, 
            UserManager<IdentityUser> userManager,
            ILogger<AdministrationController> logger)
        {
            _context = context;
            _userManager = userManager;
            this.Logger = logger;

        }

        // GET: Turns
        [Authorize(Roles = "Ingreso, Medico")]
        public async Task<IActionResult> Index()
        {
            List<Medic> medics = await _context.Medics.ToListAsync();
            List<Turn> turns;
            turns = await TurnListAsync(null);
            ViewBag.Medics = medics;
            return View(turns);
        }

        [AllowAnonymous]
        //[Authorize(Roles = "Ingreso, Medico")]
        [HttpPost]
        public async Task<IActionResult> GetTurns(DateTime? dateTurn, Guid? medicId)
        {
            List<Medic> medics = await _context.Medics.ToListAsync();
            List<Turn> turns;
            if (medicId != null)
            {
                turns = await TurnListAsync(dateTurn, medicId);
            }
            else
            {
                turns = await TurnListAsync(dateTurn);
            }
            ViewBag.Medics = medics;
            return PartialView("_TurnsPartial", turns);
        }

        public async Task<List<Turn>> TurnListAsync(DateTime? dateTurn, Guid? medicId)
        {
            ClaimsPrincipal currentUser = this.User;
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
            ViewBag.Date = dateTurn.HasValue ? String.Format("{0:yyyy-MM-dd}", dateTurn) : String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            ViewBag.IsMedic = medic != null ? true : false;
            return turns1;
        }

        public async Task<List<Turn>> TurnListAsync(DateTime? dateTurn)
        {
            ClaimsPrincipal currentUser = this.User;
            var user = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var medic = await _context.Medics.Where(m => m.UserGuid == user).FirstOrDefaultAsync();
            List<Turn> turns = new List<Turn>();
            if (medic != null)
            {
                if (dateTurn.HasValue)
                    turns = await _context.Turns.Where(m => m.MedicId == medic.Id && m.DateTurn == dateTurn).OrderBy(t => t.Time.Time).ToListAsync();
                else
                    turns = await _context.Turns.Where(m => m.MedicId == medic.Id && m.DateTurn == DateTime.Today).OrderBy(t => t.Time.Time).ToListAsync();
            }
            else
            {
                if (dateTurn.HasValue)
                    turns = await _context.Turns.Where(m => m.DateTurn == dateTurn).OrderBy(t => t.Time.Time).ToListAsync();
                else
                    turns = await _context.Turns.Where(m => m.DateTurn == DateTime.Today).OrderBy(t => t.Time.Time).ToListAsync();
            }
            List<Turn> turns1 = new List<Turn>();
            foreach (var t in turns)
            {
                t.Time = await _context.TimeTurns.FirstOrDefaultAsync(ti => ti.Id == t.TimeId);
                t.Medic = await _context.Medics.FirstOrDefaultAsync(m => m.Id == t.MedicId);
                turns1.Add(t);
            }
            ViewBag.Date = dateTurn.HasValue ? String.Format("{0:yyyy-MM-dd}", dateTurn) : String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            ViewBag.IsMedic = medic != null ? true : false;
            return turns1;
        }

        // GET: Turns/Details/5
        [Authorize(Roles = "Ingreso, Medico")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turn = await _context.Turns
                .FirstOrDefaultAsync(m => m.Id == id);
            turn.Medic = await _context.Medics.FirstOrDefaultAsync(m => m.Id == turn.MedicId);
            if (turn == null)
            {
                return NotFound();
            }

            return View(turn);
        }

        // GET: Turns/Create
        [Authorize(Roles = "Ingreso, Medico")]
        public async Task<IActionResult> Create()
        {
            ViewBag.DateTurn = DateTime.Today;
            List<Medic> medics = await _context.Medics.ToListAsync();
            List<TimeTurnViewModel> time = await _context.TimeTurns.OrderBy(t => t.Time).ToListAsync();
            ViewBag.Medics = medics;
            ViewBag.Time = time;
            return View();
        }

        // POST: Turns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Ingreso, Medico")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Turn turn)
        {
            if (ModelState.IsValid)
            {
                ClaimsPrincipal currentUser = this.User;
                var user = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                var medic = await _context.Medics.Where(m => m.UserGuid == user).FirstOrDefaultAsync();
                turn.Id = Guid.NewGuid();
                _context.Add(turn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Medico")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accessed(Guid? id)
        {
            var turn = await _context.Turns.FindAsync(id);
            if (turn == null)
            {
                ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
                return View("NotFound");
            }

            ClaimsPrincipal currentUser = this.User;
            var user = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var medic = await _context.Medics.Where(m => m.UserGuid == user).FirstOrDefaultAsync();
            turn.Medic = medic;
            turn.MedicId = medic.Id;

            if (turn.Id == null || !TurnExists(turn.Id))
            {
                ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (turn.DateTurn == DateTime.Today)
                    {
                        turn.Accessed = true;
                        _context.Update(turn);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {

                    Logger.LogError($"Error editing Turn {ex}");
                    return View("Error");
                }
            }
            List<Turn> turns = await this.TurnListAsync(null);
            return PartialView("_TurnsPartial", turns);
        }

        // GET: Turns/Edit/5
        [Authorize(Roles = "Ingreso")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var turn = await _context.Turns.FindAsync(id);
            List<Medic> medics = await _context.Medics.ToListAsync();
            List<TimeTurnViewModel> time = await _context.TimeTurns.OrderBy(t => t.Time).ToListAsync();
            ViewBag.Medic = turn.MedicId;
            ViewBag.TimeId = turn.TimeId;
            ViewBag.Medics = medics;
            ViewBag.Time = time;
            if (turn == null)
            {
                ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
                return View("NotFound");
            }
            return View(turn);
        }

        [Authorize(Roles = "Ingreso")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Turn turn)
        {
            if (turn.Id == null || !TurnExists(turn.Id))
            {
                ViewBag.ErrorMessage = $"Turn with Id = {turn.Id} cannot be found";
                return View("NotFound");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(turn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.LogError($"Error editing turn {ex}");
                    return View("Error");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(turn);
        }

        // POST: Turns/Delete/5
        [Authorize(Roles = "Admin, Ingreso")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var turn = await _context.Turns.FindAsync(id);
            _context.Turns.Remove(turn);
            await _context.SaveChangesAsync();
            List<Turn> turns = await this.TurnListAsync(null);
            return PartialView("_TurnsPartial", turns);
        }

        private bool TurnExists(Guid id)
        {
            return _context.Turns.Any(e => e.Id == id);
        }
    }
}
