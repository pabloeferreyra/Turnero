using Microsoft.AspNetCore.Mvc.Rendering;
using Turnero.Application.Services.TurnsServices;

namespace Turnero.Web.Controllers;

[Authorize(Roles = $"{RolesConstants.Ingreso}, {RolesConstants.Medico}, {RolesConstants.Admin}")]
public class DashboardController(IGetDashboardDataService dashboardService,
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    public async Task<IActionResult> Index()
    {
        // Default to last 30 days
        var endDate = DateOnly.FromDateTime(DateTime.Today);
        var startDate = endDate.AddDays(-29);

        var medics = await GetCachedMedicsAsync();

        return View(new DashboardIndexViewModel
        {
            StartDate = startDate.ToString("yyyy-MM-dd"),
            EndDate = endDate.ToString("yyyy-MM-dd"),
            Medics = medics
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GetDashboardData([FromForm] string startDate, [FromForm] string endDate, [FromForm] string? medicId)
    {
        try
        {
            DateOnly start = DateOnly.Parse(startDate);
            DateOnly end = DateOnly.Parse(endDate);

            if (start > end)
            {
                return BadRequest(new { error = "La fecha de inicio no puede ser mayor que la fecha de fin." });
            }

            if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 365)
            {
                return BadRequest(new { error = "El rango máximo permitido es de 365 días." });
            }

            Guid? parsedMedicId = !string.IsNullOrEmpty(medicId) && Guid.TryParse(medicId, out var mid) ? mid : null;
            var data = dashboardService.GetDashboardData(start, end, parsedMedicId);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error al obtener datos del dashboard: " + ex.Message });
        }
    }
}
