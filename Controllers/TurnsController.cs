namespace Turnero.Controllers;

public class TurnsController(UserManager<IdentityUser> userManager,
                       IInsertTurnsServices insertTurns,
                       IGetTurnsServices getTurns,
                       IGetTurnDTOServices getTurnDTO,
                       IUpdateTurnsServices updateTurns,
                       IGetMedicsServices getMedics,
                       IGetTimeTurnsServices getTimeTurns,
                       IHubContext<TurnsTableHub> hubContext,
                       IMemoryCache cache) : TurneroBaseController
{

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    public async Task<IActionResult> Index()
    {
        ViewBag.MedicId = await CheckMedic();
        var medics = await GetCachedMedicsAsync();
        ViewBag.Medics = new SelectList(medics, "Id", "Name");

        return View(nameof(Index));
    }

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    [HttpPost]
    public async Task<IActionResult> InitializeTurns()
    {
        string isMedic = await CheckMedic();
        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(Request);

        var medic = isMedic ?? Request.Form["Columns[5][search][value]"].FirstOrDefault();
        var dateTurnStr = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        DateOnly dateTurn = DateOnly.TryParse(dateTurnStr, out var dt) ? dt : DateOnly.FromDateTime(DateTime.Today);

        List<TurnDTO> data;
        if (!string.IsNullOrEmpty(medic))
        {
            data = [.. getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, Guid.Parse(medic))];
        }
        else
        {
            data = [.. getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, null)];
        }

        data = DataTablesHelper.ApplySorting(data, Request);
        var recordsTotal = data.Count;
        data = DataTablesHelper.ApplyPaging(data, pageSize, skip);

        foreach (var t in data)
        {
            t.IsMedic = isMedic != null;
        }

        return Ok(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
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
        var medics = await GetCachedMedicsAsync();
        var time = await GetCachedTimeTurnsAsync();
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
            turn.Date = turn.Date ?? DateTime.Today.ToString("dd-MM-yyyy");
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
            return NotFoundError("Turn", "null");
        }
        if (turn == null)
        {
            return NotFoundError("Turn", id.ToString());
        }

        if (!getTurns.Exists(turn.Id))
        {
            return NotFoundError("Turn", id.ToString());
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
            return NotFoundError("Turn", id.ToString());
        }

        var medics = await GetCachedMedicsAsync();
        var time = await GetCachedTimeTurnsAsync();

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
            return NotFoundError("Turn", turn.Id.ToString());
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
    private async Task<List<TurnDTO>> GetFilteredTurns()
    {
        string isMedic = await CheckMedic();
        var medic = isMedic ?? Request.Form["Columns[5][search][value]"].FirstOrDefault();
        var dateTurnStr = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        DateOnly dateTurn = DateOnly.TryParse(dateTurnStr, out var dt) ? dt : DateOnly.FromDateTime(DateTime.Today);
        return !string.IsNullOrEmpty(medic)
            ? [.. getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, Guid.Parse(medic))]
            : [.. getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, null)];
    }

    [HttpPost]
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return getTurns.CheckTurn(medicId, date, timeTurn);
    }    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    [HttpPost]
    public async Task<IActionResult> ExportExcel()
    {
        var data = await GetFilteredTurns();

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
        var registres = data.OrderBy(t => TimeSpan.Parse(t.Time));
        
        foreach (var t in registres)
        {
            ws.Cell(row, 1).Value = t.Name;
            ws.Cell(row, 2).Value = t.Dni;
            ws.Cell(row, 3).Value = t.SocialWork;
            ws.Cell(row, 4).Value = t.Reason;
            ws.Cell(row, 5).Value = t.MedicName;
            ws.Cell(row, 6).Value = DateTime.ParseExact(t.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 7).Value = TimeSpan.Parse(t.Time);
            ws.Cell(row, 7).Style.DateFormat.Format = "HH:mm";
            row++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var bytes = ms.ToArray();

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "turnos.xlsx");
    }    [HttpPost]
    public async Task<IActionResult> ExportPdf()
    {
        var data = await GetFilteredTurns();

        var registres = data.OrderBy(t => TimeSpan.Parse(t.Time));

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

                    foreach (var t in registres)
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