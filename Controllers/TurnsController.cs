using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using ApplicationDbContext = Turnero.Data.ApplicationDbContext;

namespace Turnero.Controllers
{
    public class TurnsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IInsertTurnsServices _insertTurns;
        private readonly ILogger<TurnsController> _logger;
        private readonly IGetTurnsServices _getTurns;
        private readonly IUpdateTurnsServices _updateTurns;
        private readonly IGetMedicsServices _getMedics;
        private readonly IGetTimeTurnsServices _getTimeTurns;

        public TurnsController(UserManager<IdentityUser> userManager,
                               ILogger<TurnsController> logger,
                               IInsertTurnsServices insertTurns,
                               IGetTurnsServices getTurns,
                               IUpdateTurnsServices updateTurns,
                               IGetMedicsServices getMedics, 
                               IGetTimeTurnsServices getTimeTurns)
        {
            _userManager = userManager;
            _logger = logger;
            _insertTurns = insertTurns;
            _getTurns = getTurns;
            _updateTurns = updateTurns;
            _getMedics = getMedics;
            _getTimeTurns = getTimeTurns;
        }

        [Authorize(Roles = "Ingreso, Medico")]
        public async Task<IActionResult> Index(int? pageNumber)
        {
            List<Medic> medics = await _getMedics.GetMedics();
            List<Turn> turns;
            turns = await TurnListAsync(null, null);
            ViewBag.Medics = medics;
            var size = 10;
            return View(PaginatedList<Turn>.Create(turns, pageNumber ?? 1, size));  
        }

        //[AllowAnonymous]
        [Authorize(Roles = "Ingreso, Medico")]
        [HttpGet]
        public async Task<IActionResult> GetTurns(DateTime? dateTurn, Guid? medicId, int? pageNumber)
        {
            List<Medic> medics = await _getMedics.GetMedics();
            List<Turn> turns;
            if (medicId != null)
            {
                turns = await TurnListAsync(dateTurn, medicId);
            }
            else
            {
                turns = await TurnListAsync(dateTurn, null);
            }
            ViewBag.Medics = medics;
            var size = 10;
            var ret = PaginatedList<Turn>.Create(turns, pageNumber ?? 1, size);
            return PartialView("_TurnsPartial", ret);
        }

        public async Task<List<Turn>> TurnListAsync(DateTime? dateTurn, Guid? medicId)
        {
            var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<Turn> turns = await this._getTurns.GetTurns(this.User, dateTurn, medicId);
            return turns;
        }

        [Authorize(Roles = "Ingreso, Medico")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turn = await this._getTurns.GetTurn((Guid)id);
            
            turn.Medic = await _getMedics.GetMedicById(turn.MedicId);
            
            if (turn == null)
            {
                return NotFound();
            }

            return View(turn);
        }

        [Authorize(Roles = "Ingreso, Medico")]
        public async Task<IActionResult> Create()
        {
            ViewBag.DateTurn = DateTime.Today;
            List<Medic> medics = await _getMedics.GetMedics();
            List<TimeTurnViewModel> time = await _getTimeTurns.GetTimeTurns();
            ViewBag.Medics = medics;
            ViewBag.Time = time;
            return View();
        }

        [Authorize(Roles = "Ingreso, Medico")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Turn turn)
        {
            if (ModelState.IsValid)
            {
                bool resInsert = await this._insertTurns.CreateTurnAsync(turn);
               if (resInsert)
                return RedirectToAction(nameof(Index));
               else
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Medico")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accessed(Guid? id, int? pageNumber)
        {
            Turn turn;
            if (id != null)
            {
                turn = await this._getTurns.GetTurn((Guid)id);
            }
            else
            {
                ViewBag.ErrorMessage = $"Turn with no id cannot be found";
                return View("NotFound");
            }
            if (turn == null)
            {
                ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
                return View("NotFound");
            }

            var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var medic = await _getMedics.GetMedicByUserId(user);
            turn.Medic = medic;
            turn.MedicId = medic.Id;

            if (!await _getTurns.Exists(turn.Id))
            {
                ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                this._updateTurns.Accessed(this.User, turn);
            }
            List<Turn> turns = await this.TurnListAsync(null, null);
            var size = 10;
            return PartialView("_TurnsPartial", PaginatedList<Turn>.Create(turns, pageNumber ?? 1, size));
        }

        [Authorize(Roles = "Ingreso")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var turn = await _getTurns.GetTurn((Guid)id);
            List<Medic> medics = await _getMedics.GetMedics();
            List<TimeTurnViewModel> time = await _getTimeTurns.GetTimeTurns();
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
            if (!await _getTurns.Exists(turn.Id))
            {
                ViewBag.ErrorMessage = $"Turn with Id = {turn.Id} cannot be found";
                return View("NotFound");
            }
            if (ModelState.IsValid)
            {
                _updateTurns.Update(turn);
                return RedirectToAction(nameof(Index));
            }
            return View(turn);
        }

        [Authorize(Roles = "Admin, Ingreso")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int? pageNumber)
        {
            var turn = await _getTurns.GetTurn(id);
            _updateTurns.Delete(turn);
            List<Turn> turns = await this.TurnListAsync(null, null);
            var size = 10;
            return PartialView("_TurnsPartial", PaginatedList<Turn>.Create(turns, pageNumber ?? 1, size));
        }
    }
}
