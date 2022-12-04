using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
namespace Turnero.Controllers;

public class TurnsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    public IInsertTurnsServices _insertTurns;
    public ILogger<TurnsController> _logger;
    public IGetTurnsServices _getTurns;
    public IUpdateTurnsServices _updateTurns;
    public IGetMedicsServices _getMedics;
    public IGetTimeTurnsServices _getTimeTurns;
    public IExportService _exportService;

    public TurnsController(UserManager<IdentityUser> userManager,
                           ILogger<TurnsController> logger,
                           IInsertTurnsServices insertTurns,
                           IGetTurnsServices getTurns,
                           IUpdateTurnsServices updateTurns,
                           IGetMedicsServices getMedics,
                           IGetTimeTurnsServices getTimeTurns,
                           IExportService exportService)
    {
        _userManager = userManager;
        _logger = logger;
        _insertTurns = insertTurns;
        _getTurns = getTurns;
        _updateTurns = updateTurns;
        _getMedics = getMedics;
        _getTimeTurns = getTimeTurns;
        _exportService = exportService;
    }

    [Authorize(Roles = "Ingreso, Medico")]
    public async Task<IActionResult> Index(int? pageNumber)
    {
        List<Medic> medics = await _getMedics.GetMedics();
        List<Turn> turns;
        turns = await TurnListAsync(null, null);
        ViewBag.Medics = medics;
        var size = 50;
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
        var size = 50;
        var ret = PaginatedList<Turn>.Create(turns, pageNumber ?? 1, size);
        return PartialView("_TurnsPartial", ret);
    }

    public async Task<List<Turn>> TurnListAsync(DateTime? dateTurn, Guid? medicId)
    {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var medic = await _getMedics.GetMedicByUserId(user);
        ViewBag.Date = dateTurn.HasValue ? String.Format("{0:yyyy-MM-dd}", dateTurn) : String.Format("{0:yyyy-MM-dd}", DateTime.Now);
        ViewBag.IsMedic = medic != null;
        if (ViewBag.IsMedic)
        {
            ViewBag.MedicId = medic.Id;
            return await this._getTurns.GetTurns(dateTurn, medic.Id);
        }
        else
        {
            ViewBag.MedicId = null;
            return await this._getTurns.GetTurns(dateTurn, medicId);
        }
    }

    [Authorize(Roles = "Ingreso, Medico")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var turn = await this._getTurns.GetTurn((Guid)id);
        
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
        ViewBag.Medics = await _getMedics.GetMedics();
        ViewBag.Time = await _getTimeTurns.GetTimeTurns();
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
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Index));
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

        if (!_getTurns.Exists(turn.Id))
        {
            ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            this._updateTurns.Accessed(turn);
        }
        List<Turn> turns = await this.TurnListAsync(null, null);
        var size = 50;
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
        ViewBag.MedicId = turn.Medic;
        ViewBag.TimeId = turn.Time;
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
    public IActionResult Edit(Turn turn)
    {
        if (!_getTurns.Exists(turn.Id))
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
        var size = 50;
        return PartialView("_TurnsPartial", PaginatedList<Turn>.Create(turns, pageNumber ?? 1, size));
    }

    [Authorize(Roles = "Ingreso, Medico")]
    public async Task<IActionResult> Export()
    {
        List<Medic> medics = await _getMedics.GetMedics();
        ViewBag.Medics = medics;
        return View();
    }

    [HttpPost, ActionName("Export")]
    public async Task<IActionResult> ExportExcelAsync(MedicTime medicTime)
    {
        DateTime date = DateTime.Today;
        var filename = "turns";
        var medicName = await _getMedics.GetMedicById(medicTime.MedicId);
        var reportname = medicName.Name+"_" + date.Year + "-" + date.Month + "-" + date.Day + ".xlsx";
        var stream = await _exportService.ExportExcelAsync(date, medicTime.MedicId, filename);
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        
        return File(stream, contentType, reportname);
    }
}
