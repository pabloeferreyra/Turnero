using System.Collections.Generic;
using System.Threading.Tasks;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class GrowthChartController(IGetGrowthChartService get, 
    IInsertGrowthChartService insert,
    IUpdateGrowthChartService update,
    IDeleteGrowthChartService delete,
    ILogger<GrowthChartController> logger) : TurneroBaseController
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
        logger.LogInformation("Initializing Growth Chart for PatientId: {PatientId}", patientId);
        try
        {
            var result = await DataTablesHelper.InitializePatientDataTablesAsync(
                Request, patientId,
                async pid => await get.Get(pid) ?? [],
                logger);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing Growth Chart for patient {PatientId}", patientId);
            return StatusCode(500, "An error occurred while initializing Growth Chart.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID del paciente es obligatorio.");
        ViewData["PatientId"] = id.Value.ToString();
        var growthChart = new GrowthChart { PatientId = id.Value };
        SetAntiforgeryToken();
        return PartialView("_Create", growthChart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(GrowthChart growthChart)
    {
        if (!ModelState.IsValid || (growthChart == null || growthChart.PatientId == Guid.Empty))
            return BadRequest();
        try
        {
            await insert.Create(growthChart);
            logger.LogInformation("Created Growth Chart entry for PatientId: {PatientId}", growthChart.PatientId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Growth Chart entry for PatientId: {PatientId}", growthChart.PatientId);
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID del registro es obligatorio.");
        var growthChart = await get.Get(id.Value);
        if (growthChart == null)
            return NotFound("Registro no encontrado.");
        SetAntiforgeryToken();
        return PartialView("_Create", growthChart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(GrowthChart growthChart)
    {
        if (!ModelState.IsValid || (growthChart == null || growthChart.Id == Guid.Empty))
            return BadRequest();
        try
        {
            await update.Edit(growthChart);
            logger.LogInformation("Updated Growth Chart entry Id: {Id}", growthChart.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Growth Chart entry Id: {Id}", growthChart.Id);
            return StatusCode(500);
        }
    }

    [HttpDelete]
    public async Task<StatusCodeResult> Delete(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest();
        try
        {
            await delete.Delete(id.Value);
            logger.LogInformation("Deleted Growth Chart entry Id: {Id}", id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting Growth Chart entry Id: {Id}", id);
            return StatusCode(500);
        }
    }


}
