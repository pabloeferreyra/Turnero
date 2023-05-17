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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;
using Turnero.Utilities;
using Microsoft.AspNetCore.SignalR;
using Turnero.Hubs;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace Turnero.Controllers;

public class TurnsController : Controller {
    private readonly UserManager<IdentityUser> _userManager;
    public IInsertTurnsServices _insertTurns;
    public ILogger<TurnsController> _logger;
    public IGetTurnsServices _getTurns;
    public IUpdateTurnsServices _updateTurns;
    public IGetMedicsServices _getMedics;
    public IGetTimeTurnsServices _getTimeTurns;
    private readonly IMapper mapper;
    private readonly IHubContext<TurnsTableHub> _hubContext;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    public IMemoryCache _cache;
    public TurnsController(UserManager<IdentityUser> userManager,
                           ILogger<TurnsController> logger,
                           IInsertTurnsServices insertTurns,
                           IGetTurnsServices getTurns,
                           IUpdateTurnsServices updateTurns,
                           IGetMedicsServices getMedics,
                           IGetTimeTurnsServices getTimeTurns,
                           IMapper mapper,
                           IHttpClientFactory httpClientFactory,
                           IConfiguration config,
                           IHubContext<TurnsTableHub> hubContext,
                           IMemoryCache cache) {
        _userManager = userManager;
        _logger = logger;
        _insertTurns = insertTurns;
        _getTurns = getTurns;
        _updateTurns = updateTurns;
        _getMedics = getMedics;
        _getTimeTurns = getTimeTurns;
        this.mapper = mapper;
        _hubContext = hubContext;
        _config = config;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    public async Task<IActionResult> Index() {

        List<MedicDto> medics = null;

        medics = _cache.Get<List<MedicDto>>("medics");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = _getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        ViewBag.Medics = new SelectList(medics, "Id", "Name");

        return View(nameof(Index));
    }

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    [HttpPost]
    public async Task<IActionResult> InitializeTurns()
    {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var isMedic = await _getMedics.GetMedicByUserId(user);

        var turns = this._getTurns.GetTurnsDto();
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = length != null ? int.Parse(length) : 0;
        int skip = start != null ? int.Parse(start) : 0;
        var medic = isMedic == null ? Request.Form["Columns[5][search][value]"].FirstOrDefault() : isMedic.Id.ToString();
        var dateTurn = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var defa = DateTime.Today.ToString("dd/MM/yyyy");

        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            turns = turns.OrderBy(sortColumn + " " + sortColumnDirection);
        }

        var data = turns.ToList();

        if (!string.IsNullOrEmpty(medic))
        {
            data = data.Where(a => a.MedicId == Guid.Parse(medic)).ToList();
        }

        if (!string.IsNullOrEmpty(dateTurn))
        {
            data = data.Where(a => a.Date == dateTurn).ToList();
        }
        else
        {
            data = data.Where(a => a.Date == defa).ToList();
        }

        int recordsTotal = data.Count;

        if (skip != 0)
        {
            data = data.Skip(skip).Take(pageSize).ToList();
        }
        else if (pageSize != -1)
        {
            data = data.Take(pageSize).ToList();
        }

        Parallel.ForEach(data, t =>
        {
            t.IsMedic = isMedic != null;
        });

        var json = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };

        return await Task.FromResult<IActionResult>(Ok(json));
    }

    public async Task<List<Turn>> TurnListAsync(DateTime? dateTurn, Guid? medicId) {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var medic = await _getMedics.GetMedicByUserId(user);
        ViewBag.Date = dateTurn.HasValue ? String.Format("{0:yyyy-MM-dd}", dateTurn) : String.Format("{0:yyyy-MM-dd}", DateTime.Now);
        ViewBag.IsMedic = medic != null;
        if (ViewBag.IsMedic) {
            ViewBag.MedicId = medic.Id;
            return await this._getTurns.GetTurns(dateTurn, medic.Id);
        }
        else {
            ViewBag.MedicId = null;
            return await this._getTurns.GetTurns(dateTurn, medicId);
        }
    }

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    public async Task<IActionResult> Details(Guid? id) {
        if (id == null) {
            return NotFound();
        }

        var turn = await this._getTurns.GetTurn((Guid)id);

        if (turn == null) {
            return NotFound();
        }

        return View(turn);
    }


    [Authorize(Roles = "Ingreso, Medico")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        List<MedicDto> medics = null;
        List<TimeTurnViewModel> time = null;

        medics = _cache.Get<List<MedicDto>>("medics");
        time = _cache.Get<List<TimeTurnViewModel>>("timeTurns");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = _getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        if (time == null)
        {

            Task timeTask = Task.Run(() =>
            {
                time = _getTimeTurns.GetCachedTimes().Result;
            });

            await timeTask;
        }
        ViewBag.Medics = new SelectList(medics, "Id", "Name");
        ViewBag.Time = new SelectList(time, "Id", "Time");

        return PartialView("_Create");
    }


    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(TurnDTO turn) {
        if (!ModelState.IsValid) return this.BadRequest();
        try
        {
            turn.Reason = turn.Reason.TrimEnd('\"');
            var t = new Turn();
            t = mapper.Map(turn, t);
            await this._insertTurns.CreateTurnAsync(t);
            var medic = await this._getMedics.GetMedicById(turn.MedicId);
            await _hubContext.Clients.User(medic.UserGuid).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); ;

            return Ok();
        }
        catch
        {
            return Conflict();
        }
         
        
    }

    [Authorize(Roles = RolesConstants.Medico)]
    [HttpPost]
    public async Task<IActionResult> Accessed(Guid? id) {
        Turn turn;
        if (id != null) {
            turn = await this._getTurns.GetTurn((Guid)id);
        }
        else {
            ViewBag.ErrorMessage = $"Turn with no id cannot be found";
            return View("NotFound");
        }
        if (turn == null) {
            ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
            return View("NotFound");
        }

        if (!_getTurns.Exists(turn.Id)) {
            ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
            return View("NotFound");
        }

        if (ModelState.IsValid) {
            this._updateTurns.Accessed(turn);
        }
        var users = await this._userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
        foreach(var u in  users) { await _hubContext.Clients.User(u.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); }
        
        return Ok();
    }

    [Authorize(Roles = RolesConstants.Ingreso)]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return null;
        }

        var turn = await _getTurns.GetTurnDTO((Guid)id);
        if (turn == null)
        {
            ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
            return null;
        }
        
        List<MedicDto> medics = null;
        List<TimeTurnViewModel> time = null;
        medics = _cache.Get<List<MedicDto>>("medics");
        time = _cache.Get<List<TimeTurnViewModel>>("timeTurns");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = _getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        if (time == null)
        {
            Task timeTask = Task.Run(() =>
            {
                time = _getTimeTurns.GetCachedTimes().Result;
            });
            await timeTask;
        }

        ViewBag.Medics = new SelectList(medics, "Id", "Name", turn.MedicId);
        ViewBag.TimeEdit = new SelectList(time, "Id", "Time", turn.TimeId);
        
        return PartialView("_Edit", turn);
    }

    [Authorize(Roles = RolesConstants.Ingreso)]
    [HttpPut]
    public async Task<IActionResult> Edit(TurnDTO turn)
    {
        if (!_getTurns.Exists(turn.Id))
        {
            ViewBag.ErrorMessage = $"Turn with Id = {turn.Id} cannot be found";
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            var t = new Turn();
            t = mapper.Map(turn, t);
            _updateTurns.Update(t);
            var users = await this._userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
            foreach (var u in users) { await _hubContext.Clients.User(u.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); }
            return Ok();
        }
        return Conflict();
    }

    [Authorize(Roles = RolesConstants.Admin + ", " + RolesConstants.Ingreso)]
    [HttpDelete, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var turn = await _getTurns.GetTurn(id);
        _updateTurns.Delete(turn);
        await _hubContext.Clients.All.SendAsync("UpdateTable", "La tabla se ha actualizado");
        return Ok();
    }
    
    [Authorize(Roles = RolesConstants.Admin +", "+ RolesConstants.Ingreso)]
    [HttpPost]
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return _getTurns.CheckTurn(medicId, date, timeTurn);
    }

    [Authorize(Roles = RolesConstants.Medico)]
    [HttpPost]
    public async Task<IActionResult> Call(Caller model)
    {
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var client = _httpClientFactory.CreateClient();
        var caller = _config["caller"];
        var request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}Home/CallNew", caller))
        {
            Content = content
        };

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            // Si la respuesta no es exitosa, puedes manejar el error aquí.
            return StatusCode((int)response.StatusCode);
        }

        // Si la respuesta es exitosa, puedes hacer algo con los datos de la respuesta.
        return Ok();
    }
}
