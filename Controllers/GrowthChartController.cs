using System.Threading.Tasks;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class GrowthChartController(IGetGrowthChartService get, 
    IInsertGrowthChartService insert,
    IUpdateGrowthChartService update,
    IDeleteGrowthChartService delete,
    ILogger<GrowthChartController> logger) : Controller
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es requerido.");
        var data = await get.Get(id.Value);
        return PartialView("_Table", data);
    }

    [HttpPost]
    public async Task<IActionResult> Initialize(Guid? patientId)
    {
        if (patientId == Guid.Empty)
            return BadRequest("El ID del paciente es requerido.");
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Initializing Growth Chart for PatientId: {PatientId}", patientId);
        try
        {
            var (draw, pageSize, skip, data, recordsTotal) = await SetTable(patientId);
            data = SetPage(pageSize, skip, data);
            var json = new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            };
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Successfully intialized Growth Chart for patient {PatientId}", patientId);
            return Ok(json);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "Error initializing Growth Chart for patient {PatientId}", patientId);
            return StatusCode(500, "An error occurred while initializing Growth Chart.");
        }
    }

    #region private methods
    private async Task<(int draw, int pageSize, int skip, List<GrowthChart> data, int recordsTotal)> SetTable(Guid? patientIdFromQuery = null)
    {
        var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
        var start = HttpContext.Request.Form["start"].FirstOrDefault();
        var length = HttpContext.Request.Form["length"].FirstOrDefault();
        string? searchCandidate = null;
        if (patientIdFromQuery.HasValue)
        {
            searchCandidate = patientIdFromQuery.Value.ToString();
            logger.LogDebug("Using patientId from query string: {PatientId}", searchCandidate);
        }
        else
        {
            
        }
        int pageSize = length != null ? Convert.ToInt32(length) : 0;
        int skip = start != null ? Convert.ToInt32(start) : 0;
        var data = await get.Get(patientId.Value);
        int recordsTotal = data.Count;
        return (Convert.ToInt32(draw), pageSize, skip, data, recordsTotal);
    }
    #endregion
}
