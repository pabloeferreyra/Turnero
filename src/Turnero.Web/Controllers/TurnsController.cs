namespace Turnero.Web.Controllers;

public class TurnsController(UserManager<IdentityUser> userManager,
                       IInsertTurnsServices insertTurns,
                       IGetTurnsServices getTurns,
                       IGetTurnDTOServices getTurnDTO,
                       IUpdateTurnsServices updateTurns,
                       IGetMedicsServices getMedics,
                       IGetTimeTurnsServices getTimeTurns,
                       IAntiforgery antiforgery,
                       IHubContext<TurnsTableHub> hubContext,
                       ILogger<TurnsController> logger) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
    public async Task<IActionResult> Index()
    {
        var medicIdTask = CheckMedic();
        var medicsTask = GetCachedMedicsAsync();
        await Task.WhenAll(medicIdTask, medicsTask);

        var model = new TurnsIndexViewModel
        {
            Medics = medicsTask.Result,
            MedicId = medicIdTask.Result,
            IsMedic = User.IsInRole(RolesConstants.Medico)
        };

        return View(model);
    }

    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitializeTurns()
    {
        var medicId = await CheckMedic();
        var isMedic = User.IsInRole(RolesConstants.Medico);
        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(Request);

        var requestedMedic = Request.Form["Columns[5][search][value]"].FirstOrDefault();
        var medic = isMedic ? medicId : requestedMedic;
        var dateTurnStr = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        DateOnly dateTurn = DateOnly.TryParse(dateTurnStr, out var dt) ? dt : DateOnly.FromDateTime(DateTime.Today);

        List<TurnDTO> data;
        if (isMedic && string.IsNullOrEmpty(medicId))
        {
            data = [];
        }
        else if (!string.IsNullOrEmpty(medic))
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
            t.IsMedic = isMedic;
        }
        logger.LogInformation("InitializeTurns called with draw={Draw}, pageSize={PageSize}, skip={Skip}, medic={Medic}, dateTurn={DateTurn}. Returning {Count} records.", draw, pageSize, skip, medic, dateTurn, data.Count);
        return Ok(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
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
        var medicsTask = GetCachedMedicsAsync();
        var timeTask = GetCachedTimeTurnsAsync();
        await Task.WhenAll(medicsTask, timeTask);

        var model = new TurnCreateViewModel
        {
            Medics = medicsTask.Result,
            Times = timeTask.Result
        };

        return PartialView("_Create", model);
    }


    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TurnDTO turn)
    {
        try
        {
            // ── Validación de campos requeridos ───────────────────────────────
            if (turn.MedicId == Guid.Empty)
            {
                logger.LogWarning("Intento de crear turno sin Médico seleccionado.");
                return Conflict(new { error = "Debe seleccionar un médico." });
            }

            if (turn.TimeId == Guid.Empty)
            {
                logger.LogWarning("Intento de crear turno sin Horario seleccionado.");
                return Conflict(new { error = "Debe seleccionar un horario." });
            }

            var selectedMedic = await GetMedics.GetMedicById(turn.MedicId);
            if (selectedMedic.Id == Guid.Empty)
            {
                logger.LogWarning("Intento de crear turno con médico inexistente: {MedicId}", turn.MedicId);
                return Conflict(new { error = "El médico seleccionado ya no existe." });
            }

            // ── Normalizar datos ───────────────────────────────────────────
            turn.Reason = string.IsNullOrWhiteSpace(turn.Reason) ? string.Empty : turn.Reason.TrimEnd('\"');
            turn.Date ??= DateTime.Today.ToString("yyyy-MM-dd");

            // ── Validación de turno duplicado ────────────────────────────────
            if (!DateOnly.TryParseExact(turn.Date, "yyyy-MM-dd", null, DateTimeStyles.None, out var parsedDate))
            {
                logger.LogWarning("Fecha inválida al crear turno: {Date}", turn.Date);
                return Conflict(new { error = "La fecha ingresada no es válida." });
            }

            var dateTurn = parsedDate.ToDateTime(TimeOnly.MinValue);
            var exists = getTurns.CheckTurn(turn.MedicId, dateTurn, turn.TimeId);
            if (exists)
            {
                logger.LogWarning("Turno duplicado: Médico {MedicId}, Fecha {Date}, Horario {TimeId}",
                    turn.MedicId, turn.Date, turn.TimeId);
                return Conflict(new { error = "El médico ya tiene un turno en esa fecha y horario." });
            }

            // ── Procesar y guardar ───────────────────────────────────────────

            var t = turn.Adapt<Turn>();
            var created = await insertTurns.CreateTurnAsync(t);

            if (!created)
            {
                logger.LogWarning("CreateTurnAsync devolvió false para Médico {MedicId}, Fecha {Date}",
                    turn.MedicId, turn.Date);
                return Conflict(new { error = "No se pudo crear el turno. Intente nuevamente." });
            }

            // ── Notificar vía SignalR e invalidar caché ─────────────────────
            var turnMsj = "se agrego un nuevo turno";

            await hubContext.Clients.User(selectedMedic.UserGuid).SendAsync("UpdateTableDirected", selectedMedic.Name, turnMsj, t.DateTurn.ToShortDateString());

            return Ok(new { message = "Turno creado correctamente." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear turno para Médico {MedicId}, Fecha {Date}",
                turn.MedicId, turn.Date);
            return Conflict(new { error = "Ocurrió un error al crear el turno." });
        }
    }

    [Authorize(Roles = RolesConstants.Medico)]
    [HttpPost]
    [ValidateAntiForgeryToken]
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
        await hubContext.Clients.Users(users.Select(u => u.Id)).SendAsync("UpdateTableDirected", "La tabla se ha actualizado");

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
        var medicsTask = GetCachedMedicsAsync();
        var timeTask = GetCachedTimeTurnsAsync();
        await Task.WhenAll(medicsTask, timeTask);

        var model = new TurnEditViewModel
        {
            Id = turn.Id,
            Name = turn.Name,
            Dni = turn.Dni,
            MedicId = turn.MedicId,
            MedicName = turn.MedicName,
            Date = turn.Date,
            Time = turn.Time,
            TimeId = turn.TimeId,
            SocialWork = turn.SocialWork,
            Reason = turn.Reason,
            Accessed = turn.Accessed,
            IsMedic = turn.IsMedic,
            Medics = medicsTask.Result,
            Times = timeTask.Result
        };

        return PartialView("_Edit", model);
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
            await hubContext.Clients.Users(users.Select(u => u.Id)).SendAsync("UpdateTableDirected", "La tabla se ha actualizado");
            return Ok();
        }
        return Conflict();
    }

    [Authorize(Roles = $"{RolesConstants.Admin}, {RolesConstants.Ingreso}")]
    [HttpDelete, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var turn = await getTurns.GetTurn(id);
        if (turn == null)
        {
            return NotFoundError("Turn", id.ToString());
        }

        updateTurns.Delete(turn);
        await hubContext.Clients.All.SendAsync("UpdateTable", "La tabla se ha actualizado");
        return Ok();
    }

    [Authorize(Roles = $"{RolesConstants.Admin}, {RolesConstants.Ingreso}")]
    private async Task<List<TurnDTO>> GetFilteredTurns()
    {
        var medicId = await CheckMedic();
        var isMedic = User.IsInRole(RolesConstants.Medico);
        var requestedMedic = Request.Form["Columns[5][search][value]"].FirstOrDefault();
        var medic = isMedic ? medicId : requestedMedic;
        var dateTurnStr = Request.Form["Columns[6][search][value]"].FirstOrDefault();
        DateOnly dateTurn = DateOnly.TryParse(dateTurnStr, out var dt) ? dt : DateOnly.FromDateTime(DateTime.Today);
        return isMedic && string.IsNullOrEmpty(medicId)
            ? []
            : !string.IsNullOrEmpty(medic)
            ? [.. getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, Guid.Parse(medic))]
            : [.. getTurnDTO.GetTurnsDtoByDateAndId(dateTurn, null)];
    }

    [HttpPost]
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return getTurns.CheckTurn(medicId, date, timeTurn);
    }    [Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportPdf()
    {
        var data = await GetFilteredTurns();

        var registres = data.OrderBy(t => TimeSpan.Parse(t.Time));

        var pdf = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text($"Turnos del día {DateTime.Today:dd/MM/yyyy}").FontSize(20).Bold();
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

    [Authorize(Roles = $"{RolesConstants.Ingreso}")]
    [HttpGet]
    public async Task<IActionResult> Availability(Guid? medicId)
    {
        var medicsTask = GetCachedMedicsAsync();
        var timesTask = GetCachedTimeTurnsAsync();
        await Task.WhenAll(medicsTask, timesTask);

        if (medicId.HasValue)
        {
            if (!GetMedics.ExistMedic(medicId.Value))
            {
                return NotFound(new { error = "El médico seleccionado no existe." });
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var schedule = (await GetMedics.GetAvailabilities(medicId.Value))
                .Where(availability => availability.DateTo >= today)
                .GroupBy(availability => new
                {
                    availability.DateFrom,
                    availability.DateTo
                })
                .OrderByDescending(group => group.Key.DateTo)
                .ThenByDescending(group => group.Key.DateFrom)
                .FirstOrDefault();

            if (schedule is null)
            {
                return Ok(new { hasData = false });
            }

            var days = new TurnAvailabilityViewModel().Days;
            foreach (var day in days)
            {
                day.IsUnavailable = true;
            }

            foreach (var availability in schedule)
            {
                var day = days.First(item => item.Day == availability.Day);
                day.AvailabilityId = availability.Id;
                day.TimeFromId = availability.TimeFromId;
                day.TimeToId = availability.TimeToId;
                day.IsUnavailable = false;
            }

            return Ok(new
            {
                hasData = true,
                dateFrom = schedule.Key.DateFrom.ToString("yyyy-MM-dd"),
                dateTo = schedule.Key.DateTo.ToString("yyyy-MM-dd"),
                days = days.Select(day => new
                {
                    day = (int)day.Day,
                    availabilityId = day.AvailabilityId,
                    timeFromId = day.TimeFromId,
                    timeToId = day.TimeToId,
                    isUnavailable = day.IsUnavailable
                })
            });
        }

        return PartialView("_Availability", new TurnAvailabilityViewModel
        {
            Medics = medicsTask.Result,
            Times = timesTask.Result
        });
    }
    [Authorize(Roles = $"{RolesConstants.Ingreso}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Availability(TurnAvailabilityViewModel model)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (model.DateTo < today)
        {
            ModelState.AddModelError(nameof(model.DateTo), "La fecha final no puede ser anterior a hoy.");
        }

        if (model.DateFrom > model.DateTo)
        {
            ModelState.AddModelError(nameof(model.DateTo), "La fecha final debe ser igual o posterior a la inicial.");
        }

        if (model.Days.Count != 7
            || model.Days.Select(day => day.Day).Distinct().Count() != 7)
        {
            ModelState.AddModelError(nameof(model.Days), "La disponibilidad semanal no es válida.");
        }

        var submittedIds = model.Days
            .Where(day => day.AvailabilityId.HasValue && day.AvailabilityId != Guid.Empty)
            .Select(day => day.AvailabilityId!.Value)
            .ToList();
        if (submittedIds.Count != submittedIds.Distinct().Count())
        {
            ModelState.AddModelError(nameof(model.Days), "La disponibilidad semanal contiene registros duplicados.");
        }

        var medicExists = model.MedicId is Guid medicId && GetMedics.ExistMedic(medicId);
        if (model.MedicId.HasValue && !medicExists)
        {
            ModelState.AddModelError(nameof(model.MedicId), "El médico seleccionado no existe.");
        }

        if (model.DateFrom < today)
        {
            var existing = medicExists
                ? await GetMedics.GetAvailabilities(model.MedicId!.Value)
                : [];
            var hasExistingStart = existing.Any(availability =>
                availability.DateFrom == model.DateFrom
                && availability.DateTo >= today
                && model.Days.Any(day => day.AvailabilityId == availability.Id && day.Day == availability.Day));
            if (!hasExistingStart)
            {
                ModelState.AddModelError(nameof(model.DateFrom), "La fecha inicial no puede ser anterior a hoy.");
            }
        }

        var availableDays = model.Days.Where(day => !day.IsUnavailable).ToList();
        if (availableDays.Count == 0)
        {
            ModelState.AddModelError(nameof(model.Days), "Seleccione horarios para al menos un día.");
        }

        var times = await GetCachedTimeTurnsAsync();
        var timesById = times.ToDictionary(time => time.Id);
        for (var i = 0; i < model.Days.Count; i++)
        {
            var day = model.Days[i];
            if (day.IsUnavailable)
            {
                continue;
            }

            if (day.TimeFromId is null || day.TimeToId is null)
            {
                ModelState.AddModelError($"Days[{i}]", $"Complete los horarios de {day.Name}.");
                continue;
            }

            if (!timesById.TryGetValue(day.TimeFromId.Value, out var timeFrom)
                || !timesById.TryGetValue(day.TimeToId.Value, out var timeTo))
            {
                ModelState.AddModelError($"Days[{i}]", $"El horario de {day.Name} no existe.");
                continue;
            }

            if (string.Compare(timeFrom.Time, timeTo.Time, StringComparison.Ordinal) >= 0)
            {
                ModelState.AddModelError($"Days[{i}]", $"El horario final de {day.Name} debe ser posterior al inicial.");
            }
        }

        if (!ModelState.IsValid)
        {
            var medicsTask = GetCachedMedicsAsync();
            await medicsTask;

            model.Medics = medicsTask.Result;
            model.Times = times;

            return UnprocessableEntity(ModelState);
        }

        var availabilities = availableDays.Select(day => new MedicAvailability
        {
            MedicId = model.MedicId!.Value,
            DateFrom = model.DateFrom!.Value,
            DateTo = model.DateTo!.Value,
            Day = day.Day,
            Id = day.AvailabilityId ?? Guid.Empty,
            TimeFromId = day.TimeFromId!.Value,
            TimeToId = day.TimeToId!.Value
        }).ToList();

        var unavailableIds = model.Days
            .Where(day => day.IsUnavailable && day.AvailabilityId.HasValue)
            .Select(day => day.AvailabilityId!.Value)
            .Where(id => id != Guid.Empty)
            .ToList();

        await GetMedics.SaveAvailabilities(model.MedicId!.Value, availabilities, unavailableIds);

        return Ok(new { message = "Disponibilidad guardada correctamente." });
    }
}