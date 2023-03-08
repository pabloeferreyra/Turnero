using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using System.Linq;
using AutoMapper;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

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
    private readonly IMapper mapper;

    public TurnsController(UserManager<IdentityUser> userManager,
                           ILogger<TurnsController> logger,
                           IInsertTurnsServices insertTurns,
                           IGetTurnsServices getTurns,
                           IUpdateTurnsServices updateTurns,
                           IGetMedicsServices getMedics,
                           IGetTimeTurnsServices getTimeTurns,
                           IExportService exportService, IMapper mapper)
    {
        _userManager = userManager;
        _logger = logger;
        _insertTurns = insertTurns;
        _getTurns = getTurns;
        _updateTurns = updateTurns;
        _getMedics = getMedics;
        _getTimeTurns = getTimeTurns;
        _exportService = exportService;
        this.mapper = mapper;
    }

    [Authorize(Roles = "Ingreso, Medico")]
    public async Task<IActionResult> Index()
    {
        var medics = await _getMedics.GetMedicsDto();
        ViewBag.Medics = new SelectList(medics, "Name", "Name");
        return View(nameof(Index));  
    }

    //[AllowAnonymous]
    [Authorize(Roles = "Ingreso, Medico")]
    [HttpPost]
    public async Task<IActionResult> GetTurns(DateTime? dateTurn, Guid? medicId)
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
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = length != null ? Convert.ToInt32(length) : 0;
        int skip = start != null ? Convert.ToInt32(start) : 0;
        int recordsTotal = 0;
        List<TurnDTO> turnDTOs = new List<TurnDTO>();
        foreach (var t in turns) {
            turnDTOs.Add(new TurnDTO {
                Id = t.Id,
                Accessed = t.Accessed,
                Date = t.DateTurn.ToString("dd/MM/yyyy"),
                Dni = t.Dni,
                MedicName = t.Medic.Name,
                Time = t.Time.Time,
                Name = t.Name,
                Reason = t.Reason,
                SocialWork = t.SocialWork
            });
        }

        recordsTotal = turnDTOs.Count();
        var data = turnDTOs.Skip(skip).Take(pageSize).ToList();

        var json = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
        return Ok(json);
    }

    [Authorize(Roles = "Ingreso, Medico")]
    [HttpPost]
    public async Task<IActionResult> InitializeTurns() {
        var turns = this._getTurns.GetTurnsDto();
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = length != null ? Convert.ToInt32(length) : 0;
        int skip = start != null ? Convert.ToInt32(start) : 0;
        var medic = Request.Form["Columns[4][search][value]"].FirstOrDefault();
        var dateTurn = Request.Form["Columns[5][search][value]"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var defa = DateTime.Today.ToString("dd/MM/yyyy");
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection))) {
            turns = turns.OrderBy(sortColumn + " " + sortColumnDirection);
        }
        var data = turns.ToList();
        if (!string.IsNullOrEmpty(medic)) {
            data = data.Where(a => a.MedicName.Contains(medic)).ToList();
        }
        if (!string.IsNullOrEmpty(dateTurn)) {
            data = data.Where(a => a.Date == dateTurn).ToList();
        }
        else {
            data = data.Where(a => a.Date == defa).ToList();
        }
        int recordsTotal = data.Count();
        if (skip != 0) {
            data = data.Skip(skip).Take(pageSize).ToList();
        }
        else {
            data = data.Take(pageSize).ToList();
        }


        var json = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };

        return await Task.FromResult<IActionResult>(Ok(json));
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

    public async Task<List<TurnDTO>> TurnListDto() {
            return await this._getTurns.GetTurnsDto().ToListAsync();
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
    public async Task<IActionResult> Accessed(Guid? id)
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
        return PartialView("_TurnsPartial", await this.TurnListAsync(null, null));
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
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var turn = await _getTurns.GetTurn(id);
        _updateTurns.Delete(turn);
        return PartialView("_TurnsPartial", await this.TurnListAsync(null, null));
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
    
    [Authorize(Roles = "Admin, Ingreso")]
    [HttpPost]
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return _getTurns.CheckTurn(medicId, date, timeTurn);
    }
}
