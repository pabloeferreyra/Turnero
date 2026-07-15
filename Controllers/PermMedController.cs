namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class PermMedController(IGetPermMedService get,
    IInsertPermMedService insert,
    IDeletePermMedService delete,
    ILogger<PermMedController> logger) : TurneroBaseController
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
        logger.LogInformation("Initializing PermMed for PatientId: {PatientId}", patientId);

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
            logger.LogError(ex, "Error initializing Permanent medication for patient {PatientId}", patientId);
            return StatusCode(500, "An error occurred while initializing Permanent medication.");
        }
    }

    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID del paciente es obligatorio.");
        ViewData["PatientId"] = id.Value.ToString();
        var permed = new PermMed { PatientId = id.Value };
        SetAntiforgeryToken();
        return PartialView("_Create", permed);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(PermMed permMed)
    {
        if (permMed == null || permMed.PatientId == Guid.Empty)
            return BadRequest();
        try
        {
            await insert.Create(permMed);
            logger.LogInformation("Permanent medication created successfully for patient {PatientId}", permMed.PatientId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Permanent medication for patient {PatientId}", permMed.PatientId);
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
            logger.LogInformation("Permanent medication with ID {PermMedId} deleted successfully", id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting Permanent medication with ID {PermMedId}", id);
            return StatusCode(500);

        }
    }


}
