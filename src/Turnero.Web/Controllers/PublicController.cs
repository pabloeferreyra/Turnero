using Microsoft.AspNetCore.Cors;

namespace Turnero.Web.Controllers;

[ApiController]
[Route("TurnsPublic")]
[AllowAnonymous]
[RequestSizeLimit(16_384)]
public sealed class PublicController(
    IPublicTurnBookingService bookingService,
    IInsertTurnsServices insertTurns,
    IGetTurnsServices getTurns,
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    ILogger<PublicController> logger) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var medicsTask = getMedics.GetCachedMedics();
        var timesTask = getTimeTurns.GetCachedTimes();
        await Task.WhenAll(medicsTask, timesTask);

        return View("~/Views/TurnsPublic/Index.cshtml", new TurnPublicIndexViewModel
        {
            Medics = medicsTask.Result,
            Times = timesTask.Result
        });
    }

    [HttpGet("/api/public/turns/medics")]
    [AllowAnonymous]
    [EnableCors("PublicTurnClient")]
    public async Task<IActionResult> GetMedics()
    {
        var medics = await bookingService.GetMedicsAsync();
        return Ok(medics.Select(medic => new PublicMedicResponse(medic.Id, medic.Name)));
    }

    [HttpGet("/api/public/turns/availability")]
    [AllowAnonymous]
    [EnableCors("PublicTurnClient")]
    public async Task<IActionResult> GetAvailability(Guid medicId, DateOnly date)
    {
        if (!ModelState.IsValid || medicId == Guid.Empty || date == default)
        {
            return BadRequest(new { error = "Verifica el profesional y la fecha." });
        }

        var times = await bookingService.GetAvailableTimesAsync(medicId, date);
        return Ok(times.Select(time => new PublicTimeResponse(time.Id, time.Time)));
    }

    [HttpPost("/api/public/turns")]
    [AllowAnonymous]
    [EnableCors("PublicTurnClient")]
    [EnableRateLimiting("PublicTurnCreation")]
    [Consumes("application/json")]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> Create([FromBody] CreatePublicTurnRequest? request)
    {
        if (request is null || !ModelState.IsValid)
        {
            return BadRequest(new { error = "Los datos del turno no son válidos." });
        }

        if (string.IsNullOrWhiteSpace(request.Name)
            || request.MedicId == Guid.Empty
            || request.TimeId == Guid.Empty
            || !DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date)
            || date < DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { error = "Verifica los datos y la fecha del turno." });
        }

        var turn = new Turn
        {
            Name = request.Name.Trim(),
            Dni = request.Dni,
            MedicId = request.MedicId,
            DateTurn = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TimeId = request.TimeId,
            SocialWork = request.SocialWork?.Trim(),
            Reason = request.Reason?.Trim(),
            Phone = request.Phone
        };
        var result = await bookingService.CreateAsync(turn, date);
        return result switch
        {
            PublicTurnBookingResult.Created => Ok(new { message = "Turno creado correctamente." }),
            PublicTurnBookingResult.InvalidTime => BadRequest(new { error = "El horario seleccionado no existe." }),
            PublicTurnBookingResult.Unavailable => Conflict(new { error = "Ese horario ya no está disponible." }),
            _ => throw new InvalidOperationException("Resultado de reserva desconocido.")
        };
    }

    [HttpPost("Create")]
    [EnableRateLimiting("PublicTurnCreation")]
    [ValidateAntiForgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromForm] CreatePublicTurnRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "El nombre es obligatorio.");
            return ValidationProblem(ModelState);
        }

        if (request.MedicId == Guid.Empty || request.TimeId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(request.MedicId), "Debe seleccionar un médico y un horario.");
            return ValidationProblem(ModelState);
        }

        if (!DateOnly.TryParseExact(
                request.Date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            ModelState.AddModelError(nameof(request.Date), "La fecha debe tener el formato yyyy-MM-dd.");
            return ValidationProblem(ModelState);
        }

        var medic = await getMedics.GetMedicById(request.MedicId);
        if (medic.Id != request.MedicId || !getTimeTurns.TimeTurnViewModelExists(request.TimeId))
        {
            return BadRequest(new { error = "El médico o el horario seleccionado no existe." });
        }

        var dateTurn = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        if (getTurns.CheckTurn(request.MedicId, dateTurn, request.TimeId))
        {
            return Conflict(new { error = "Ese horario ya no está disponible." });
        }

        var turn = new Turn
        {
            Name = request.Name.Trim(),
            Dni = request.Dni,
            MedicId = request.MedicId,
            DateTurn = dateTurn,
            TimeId = request.TimeId,
            SocialWork = request.SocialWork?.Trim(),
            Reason = request.Reason?.Trim(),
            Phone = request.Phone
        };

        try
        {
            if (!await insertTurns.CreateTurnAsync(turn))
            {
                logger.LogError("Public turn creation failed for medic {MedicId}, date {Date}.",
                    request.MedicId, request.Date);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "No se pudo crear el turno. Intente nuevamente." });
            }
        }
        catch (TurnSlotConflictException)
        {
            return Conflict(new { error = "Ese horario acaba de ser reservado. Seleccione otro." });
        }

        return Ok(new { message = "Turno creado correctamente." });
    }
}
