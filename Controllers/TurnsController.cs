namespace Turnero.Controllers;

public class TurnsController(UserManager<IdentityUser> userManager,
                       IInsertTurnsServices insertTurns,
                       IGetTurnsServices getTurns,
                       IGetTurnDTOServices getTurnDTO,
                       IUpdateTurnsServices updateTurns,
                       IGetMedicsServices getMedics,
                       IGetTimeTurnsServices getTimeTurns,
                       IHubContext<TurnsTableHub> hubContext,
                       IMemoryCache cache) : Controller
{

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    public async Task<IActionResult> Index()
    {
        ViewBag.MedicId = await CheckMedic();
        List<MedicDto> medics = null;

        medics = cache.Get<List<MedicDto>>("medics");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = getMedics.GetCachedMedics().Result;
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

        _ = SetTable(isMedic, out string draw, out int pageSize, out int skip, out List<TurnDTO> data, out int recordsTotal);

        data = SetPage(pageSize, skip, data);

        foreach (var t in data)
        {
            t.IsMedic = isMedic != null;
        }

        var json = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };

        return await Task.FromResult<IActionResult>(Ok(json));
    }

    private IQueryable<TurnDTO> SetTable(string isMedic, out string draw, out int pageSize, out int skip, out List<TurnDTO> data, out int recordsTotal)
    {
        draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        pageSize = length == null || int.Parse(length) == 0 ? -1 : int.Parse(length);
        skip = start != null ? int.Parse(start) : 0;
        var medic = isMedic ?? Request.Form["Columns[5][search][value]"].FirstOrDefault();

        var dateTurnStr = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        _ = DateOnly.MinValue;
        DateOnly dateTurn;
        if (!string.IsNullOrEmpty(dateTurnStr) && DateOnly.TryParse(dateTurnStr, out var dt))
        {
            dateTurn = dt;
        }
        else
        {
            dateTurn = DateOnly.Parse(DateTime.Today.ToShortDateString());
        }

        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

        IQueryable<TurnDTO> turns;
        if ((dateTurn != DateOnly.MinValue) && !string.IsNullOrEmpty(medic))
        {
            turns = getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, Guid.Parse(medic));
        }
        else if((dateTurn != DateOnly.MinValue) && string.IsNullOrEmpty(medic))
        {
            turns = getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, null);
        }
        else
        {
            turns = getTurnDTO.GetTurnsDto();
        }
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            turns = turns.OrderBy(sortColumn + " " + sortColumnDirection);
        }

        data = [.. turns];
        recordsTotal = data.Count;
        return turns;
    }

    private static List<TurnDTO> SetPage(int pageSize, int skip, List<TurnDTO> data)
    {
        if (skip != 0)
        {
            data = [.. data.Skip(skip).Take(pageSize)];
        }
        else if (pageSize != -1)
        {
            data = [.. data.Take(pageSize)];
        }
        return data;
    }

    private async Task<string> CheckMedic()
    {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var isMedic = await getMedics.GetMedicByUserId(user);
        return isMedic?.Id.ToString();
    }

    public async Task<List<Turn>> TurnListAsync(DateTime? dateTurn, Guid? medicId)
    {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var medic = await getMedics.GetMedicByUserId(user);
        ViewBag.Date = dateTurn.HasValue ? String.Format("{0:yyyy-MM-dd}", dateTurn) : String.Format("{0:yyyy-MM-dd}", DateTime.Now);
        ViewBag.IsMedic = medic != null;
        if (ViewBag.IsMedic)
        {
            ViewBag.MedicId = medic.Id;
            return getTurns.GetTurns(dateTurn, medic.Id);
        }
        else
        {
            ViewBag.MedicId = null;
            return getTurns.GetTurns(dateTurn, medicId);
        }
    }

    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var turn = await getTurns.GetTurn((Guid)id);

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

        medics = cache.Get<List<MedicDto>>("medics");
        time = cache.Get<List<TimeTurn>>("timeTurns");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        if (time.Count == 0)
        {

            Task timeTask = Task.Run(() =>
            {
                time = getTimeTurns.GetCachedTimes().Result;
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
            turn.Reason = string.IsNullOrWhiteSpace(turn.Reason) ? string.Empty : turn.Reason.TrimEnd('\"');
            var t = turn.Adapt<Turn>();
            await insertTurns.CreateTurnAsync(t);
            var medic = await getMedics.GetMedicById(turn.MedicId);
            var turnMsj = "se agrego un nuevo turno";

            await hubContext.Clients.User(medic.UserGuid).SendAsync("UpdateTableDirected", medic.Name, turnMsj, t.DateTurn.ToShortDateString());

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
            turn = await getTurns.GetTurn((Guid)id);
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

        if (!getTurns.Exists(turn.Id))
        {
            ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            updateTurns.Accessed(turn);
        }
        var users = await userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
        foreach (var u in users) { await hubContext.Clients.User(u.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); }

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

        var turn = await getTurns.GetTurnDTO((Guid)id);
        if (turn == null)
        {
            ViewBag.ErrorMessage = $"Turn with Id = {id} cannot be found";
            return null;
        }

        List<MedicDto> medics = null;
        List<TimeTurn> time = null;
        medics = cache.Get<List<MedicDto>>("medics");
        time = cache.Get<List<TimeTurn>>("timeTurns");
        if (medics == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        if (time == null)
        {
            Task timeTask = Task.Run(() =>
            {
                time = getTimeTurns.GetCachedTimes().Result;
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
        if (!getTurns.Exists(turn.Id))
        {
            ViewBag.ErrorMessage = $"Turn with Id = {turn.Id} cannot be found";
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            var t = turn.Adapt<Turn>();

            updateTurns.Update(t);
            var users = await userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
            foreach (var u in users) { await hubContext.Clients.User(u.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); }
            return Ok();
        }
        return Conflict();
    }

    [Authorize(Roles = RolesConstants.Admin + ", " + RolesConstants.Ingreso)]
    [HttpDelete, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var turn = await getTurns.GetTurn(id);
        updateTurns.Delete(turn);
        await hubContext.Clients.All.SendAsync("UpdateTable", "La tabla se ha actualizado");
        return Ok();
    }

    [Authorize(Roles = RolesConstants.Admin + ", " + RolesConstants.Ingreso)]
    [HttpPost]
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return getTurns.CheckTurn(medicId, date, timeTurn);
    }

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    [HttpPost]
    public async Task<IActionResult> ExportExcel()
    {
        string isMedic = await CheckMedic();

        _ = SetTable(isMedic, out _, out _, out _, out List<TurnDTO> data, out _).OrderBy(p => p.Time);

        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.AddWorksheet("Turnos");

        ws.Cell(1, 1).Value = "Nombre";
        ws.Cell(1, 2).Value = "DNI";
        ws.Cell(1, 3).Value = "Obra Social";
        ws.Cell(1, 4).Value = "Motivo";
        ws.Cell(1, 5).Value = "Médico";
        ws.Cell(1, 6).Value = "Fecha";
        ws.Cell(1, 7).Value = "Hora";

        int row = 2;
        foreach (var t in data)
        {
            ws.Cell(row, 1).Value = t.Name;
            ws.Cell(row, 2).Value = t.Dni;
            ws.Cell(row, 3).Value = t.SocialWork;
            ws.Cell(row, 4).Value = t.Reason;
            ws.Cell(row, 5).Value = t.MedicName;
            ws.Cell(row, 6).Value = t.Date;
            ws.Cell(row, 7).Value = t.Time;
            row++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var bytes = ms.ToArray();

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "turnos.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportPdf()
    {
        string isMedic = await CheckMedic();
        _ = SetTable(isMedic, out _, out _, out _, out List<TurnDTO> data, out _);

        var pdf = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text("Turnos del día").FontSize(20).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Nombre").Bold();
                        header.Cell().Text("DNI").Bold();
                        header.Cell().Text("Obra").Bold();
                        header.Cell().Text("Médico").Bold();
                        header.Cell().Text("Hora").Bold();
                    });

                    foreach (var t in data)
                    {
                        table.Cell().Text(t.Name);
                        table.Cell().Text(t.Dni);
                        table.Cell().Text(t.SocialWork);
                        table.Cell().Text(t.MedicName);
                        table.Cell().Text(t.Time);
                    }
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", "turnos.pdf");
    }
}