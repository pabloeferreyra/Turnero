﻿namespace Turnero.Controllers;

public class TurnsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    public IInsertTurnsServices _insertTurns;
    public ILogger<TurnsController> _logger;
    public IGetTurnsServices _getTurns;
    public IGetTurnDTOServices _getTurnDTO;
    public IUpdateTurnsServices _updateTurns;
    public IGetMedicsServices _getMedics;
    public IGetTimeTurnsServices _getTimeTurns;
    private readonly IMapper mapper;
    private readonly IHubContext<TurnsTableHub> _hubContext;
    public IMemoryCache _cache;
    public TurnsController(UserManager<IdentityUser> userManager,
                           ILogger<TurnsController> logger,
                           IInsertTurnsServices insertTurns,
                           IGetTurnsServices getTurns,
                           IGetTurnDTOServices getTurnDTO,
                           IUpdateTurnsServices updateTurns,
                           IGetMedicsServices getMedics,
                           IGetTimeTurnsServices getTimeTurns,
                           IMapper mapper,
                           IHubContext<TurnsTableHub> hubContext,
                           IMemoryCache cache)
    {
        _userManager = userManager;
        _logger = logger;
        _insertTurns = insertTurns;
        _getTurns = getTurns;
        _updateTurns = updateTurns;
        _getMedics = getMedics;
        _getTimeTurns = getTimeTurns;
        this.mapper = mapper;
        _hubContext = hubContext;
        _cache = cache;
        _getTurnDTO = getTurnDTO;
    }

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    public async Task<IActionResult> Index()
    {

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

        string isMedic = await CheckMedic();
        var turns = this._getTurnDTO.GetTurnsDto();
        _ = SetTable(isMedic, turns, out string draw, out int pageSize, out int skip, out List<TurnDTO> data, out int recordsTotal);

        data = SetPage(pageSize, skip, data);

        foreach (var t in data)
        {
            t.IsMedic = isMedic != null;
        }

        var json = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };

        return await Task.FromResult<IActionResult>(Ok(json));
    }

    private IQueryable<TurnDTO> SetTable(string isMedic, IQueryable<TurnDTO> turns, out string draw, out int pageSize, out int skip, out List<TurnDTO> data, out int recordsTotal)
    {
        draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        pageSize = length != null ? int.Parse(length) : 0;
        skip = start != null ? int.Parse(start) : 0;
        var medic = isMedic ?? Request.Form["Columns[5][search][value]"].FirstOrDefault();
        var dateTurn = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        var defa = DateTime.Today.ToString("dd/MM/yyyy");

        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            turns = turns.OrderBy(sortColumn + " " + sortColumnDirection);
        }

        data = turns.ToList();
        if (!string.IsNullOrEmpty(medic))
        {
            data = data.Where(a => a.MedicId == Guid.Parse(medic)).ToList();
        }

        data = !string.IsNullOrEmpty(dateTurn) ? data.Where(a => a.Date == dateTurn).ToList() : data.Where(a => a.Date == defa).ToList();

        recordsTotal = data.Count;
        return turns;
    }

    private static List<TurnDTO> SetPage(int pageSize, int skip, List<TurnDTO> data)
    {
        if (skip != 0)
        {
            data = data.Skip(skip).Take(pageSize).ToList();
        }
        else if (pageSize != -1)
        {
            data = data.Take(pageSize).ToList();
        }

        return data;
    }

    private async Task<string> CheckMedic()
    {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var isMedic = await _getMedics.GetMedicByUserId(user);
        return isMedic?.Id.ToString();
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
            return this._getTurns.GetTurns(dateTurn, medic.Id);
        }
        else
        {
            ViewBag.MedicId = null;
            return this._getTurns.GetTurns(dateTurn, medicId);
        }
    }

    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
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


    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        List<MedicDto> medics = null;
        List<TimeTurn> time = null;

        medics = _cache.Get<List<MedicDto>>("medics");
        time = _cache.Get<List<TimeTurn>>("timeTurns");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = _getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        if (time.Count == 0)
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
    public async Task<StatusCodeResult> Create(TurnDTO turn)
    {
        try
        {
            turn.Reason = turn.Reason.TrimEnd('\"');
            var t = new Turn();
            t = mapper.Map(turn, t);
            await this._insertTurns.CreateTurnAsync(t);
            var medic = await this._getMedics.GetMedicById(turn.MedicId);
            var turnMsj = "se agrego un nuevo turno";

            await _hubContext.Clients.User(medic.UserGuid).SendAsync("UpdateTableDirected", medic.Name, turnMsj, t.DateTurn.ToShortDateString()); ;

            return Ok();
        }
        catch
        {
            return Conflict();
        }


    }

    [Authorize(Roles = RolesConstants.Medico)]
    [HttpPost]
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
        var users = await this._userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
        foreach (var u in users) { await _hubContext.Clients.User(u.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); }

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
        List<TimeTurn> time = null;
        medics = _cache.Get<List<MedicDto>>("medics");
        time = _cache.Get<List<TimeTurn>>("timeTurns");
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

    [Authorize(Roles = RolesConstants.Admin + ", " + RolesConstants.Ingreso)]
    [HttpPost]
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return _getTurns.CheckTurn(medicId, date, timeTurn);
    }
}