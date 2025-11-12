using System.Globalization;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Turnero.Controllers.Api.Dtos;
using Turnero.DAL.Models;
using Turnero.Hubs;
using Turnero.SL.Services.Interfaces.Medics;
using Turnero.SL.Services.Interfaces.Turns;
using Turnero.Utilities.Constants;

namespace Turnero.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
public class TurnsApiController : ControllerBase
{
    private readonly IInsertTurnsServices insertTurns;
    private readonly IUpdateTurnsServices updateTurns;
    private readonly IGetTurnsServices getTurns;
    private readonly IGetTurnDTOServices getTurnDtos;
    private readonly IGetMedicsServices getMedics;
    private readonly IHubContext<TurnsTableHub> hubContext;
    private readonly UserManager<IdentityUser> userManager;

    public TurnsApiController(
        IInsertTurnsServices insertTurns,
        IUpdateTurnsServices updateTurns,
        IGetTurnsServices getTurns,
        IGetTurnDTOServices getTurnDtos,
        IGetMedicsServices getMedics,
        IHubContext<TurnsTableHub> hubContext,
        UserManager<IdentityUser> userManager)
    {
        this.insertTurns = insertTurns;
        this.updateTurns = updateTurns;
        this.getTurns = getTurns;
        this.getTurnDtos = getTurnDtos;
        this.getMedics = getMedics;
        this.hubContext = hubContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TurnResponse>>> GetTurnsAsync(
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? medicId,
        [FromQuery] bool includeAccessed = false)
    {
        var turnsQuery = getTurnDtos.GetTurnsDto();

        if (date.HasValue)
        {
            var formatted = date.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            turnsQuery = turnsQuery.Where(t => t.Date == formatted);
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.Today).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            turnsQuery = turnsQuery.Where(t => t.Date == today);
        }

        if (medicId.HasValue)
        {
            turnsQuery = turnsQuery.Where(t => t.MedicId == medicId.Value);
        }

        if (!includeAccessed)
        {
            turnsQuery = turnsQuery.Where(t => !t.Accessed);
        }

        var turns = turnsQuery
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Time)
            .ToList();

        var response = turns
            .Select(t =>
            {
                var parsedDate = TryParseDate(t.Date);
                return new TurnResponse(
                    t.Id,
                    t.Name ?? string.Empty,
                    t.Dni ?? string.Empty,
                    t.MedicId,
                    t.MedicName,
                    parsedDate ?? DateOnly.FromDateTime(DateTime.Today),
                    t.TimeId,
                    t.Time ?? string.Empty,
                    t.SocialWork,
                    t.Reason,
                    t.Accessed);
            })
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTurnAsync([FromBody] TurnCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var turn = new Turn
        {
            Name = request.Name.Trim(),
            Dni = request.Dni.Trim(),
            MedicId = request.MedicId,
            DateTurn = request.DateTurn.ToDateTime(TimeOnly.MinValue),
            TimeId = request.TimeId,
            SocialWork = request.SocialWork,
            Reason = request.Reason,
            Accessed = false
        };

        await insertTurns.CreateTurnAsync(turn);

        var medic = await getMedics.GetMedicById(turn.MedicId);
        if (medic != null)
        {
            await hubContext.Clients.User(medic.UserGuid)
                .SendAsync("UpdateTableDirected", medic.Name, "Se agregó un nuevo turno", turn.DateTurn.ToShortDateString());
        }

        var response = await MapTurnToResponseAsync(turn.Id);
        return CreatedAtAction(nameof(GetTurnsAsync), new { turn.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTurnAsync(Guid id, [FromBody] TurnUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (id != request.Id)
        {
            return BadRequest("El identificador no coincide con el turno a actualizar.");
        }

        var turn = await getTurns.GetTurn(id);
        if (turn is null || turn.Id == Guid.Empty)
        {
            return NotFound();
        }

        turn.Name = request.Name.Trim();
        turn.Dni = request.Dni.Trim();
        turn.MedicId = request.MedicId;
        turn.DateTurn = request.DateTurn.ToDateTime(TimeOnly.MinValue);
        turn.TimeId = request.TimeId;
        turn.SocialWork = request.SocialWork;
        turn.Reason = request.Reason;
        turn.Accessed = request.Accessed;

        updateTurns.Update(turn);

        var users = await userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
        foreach (var user in users)
        {
            await hubContext.Clients.User(user.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado");
        }

        var response = await MapTurnToResponseAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RolesConstants.Admin + ", " + RolesConstants.Ingreso)]
    public async Task<IActionResult> DeleteTurnAsync(Guid id)
    {
        var turn = await getTurns.GetTurn(id);
        if (turn is null || turn.Id == Guid.Empty)
        {
            return NotFound();
        }

        updateTurns.Delete(turn);
        await hubContext.Clients.All.SendAsync("UpdateTable", "La tabla se ha actualizado");
        return NoContent();
    }

    [HttpPost("{id:guid}/accessed")]
    [Authorize(Roles = RolesConstants.Medico + ", " + RolesConstants.Ingreso)]
    public async Task<IActionResult> MarkAccessedAsync(Guid id)
    {
        var turn = await getTurns.GetTurn(id);
        if (turn is null || turn.Id == Guid.Empty)
        {
            return NotFound();
        }

        updateTurns.Accessed(turn);

        var users = await userManager.GetUsersInRoleAsync(RolesConstants.Ingreso);
        foreach (var user in users)
        {
            await hubContext.Clients.User(user.Id).SendAsync("UpdateTableDirected", "La tabla se ha actualizado");
        }

        return NoContent();
    }

    private async Task<TurnResponse> MapTurnToResponseAsync(Guid id)
    {
        var turn = await getTurns.GetTurn(id);
        if (turn is null || turn.Id == Guid.Empty)
        {
            throw new InvalidOperationException($"No se encontró el turno con Id {id}");
        }

        var medic = await getMedics.GetMedicById(turn.MedicId);
        var dto = turn.Adapt<TurnDTO>();
        dto.MedicName = medic?.Name;
        dto.Time = turn.Time?.Time;
        dto.Date = turn.DateTurn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        return new TurnResponse(
            dto.Id,
            dto.Name ?? string.Empty,
            dto.Dni ?? string.Empty,
            dto.MedicId,
            dto.MedicName,
            DateOnly.FromDateTime(turn.DateTurn),
            dto.TimeId,
            dto.Time ?? string.Empty,
            dto.SocialWork,
            dto.Reason,
            dto.Accessed);
    }

    private static DateOnly? TryParseDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return DateOnly.FromDateTime(parsed);
        }

        if (DateTime.TryParse(date, out var fallback))
        {
            return DateOnly.FromDateTime(fallback);
        }

        return null;
    }
}
