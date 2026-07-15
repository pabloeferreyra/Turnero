namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class VaccinesController(IGetVaccinesServices get,
    IUpdateVaccinesServices update,
    IInsertVaccinesServices insert,
    IDeleteVacinesServices delete,
    ILogger<VaccinesController> logger) : TurneroBaseController
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
        {
            return BadRequest("El ID del paciente es obligatorio.");
        }
        var data = await get.GetByPatientId(id.Value);
        return PartialView("_Table", data);
    }

    [HttpPost]
    public async Task<IActionResult> InitializeVaccines(Guid? patientId)
    {
        logger.LogInformation("Initializing vaccines for patient {PatientId}", patientId);
        if (patientId == Guid.Empty)
            return BadRequest("Patient ID is required.");

        try
        {
            var result = await DataTablesHelper.InitializePatientDataTablesAsync(
                Request, patientId,
                async pid => await get.GetByPatientId(pid) ?? [],
                logger);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing vaccines for patient {PatientId}", patientId);
            return StatusCode(500, "An error occurred while initializing vaccines.");
        }
    }


    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID del paciente es obligatorio.");
        ViewData["PatientId"] = id.Value.ToString();
        var vaccine = new Vaccines { PatientId = id.Value };
        SetAntiforgeryToken();
        ViewBag.Description = EnumNamesToSelectList<VaccinesEnum>(name => name.Replace("_", " "));
        return PartialView("_Create", vaccine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(VaccinesDto dto)
    {
        if (dto == null || dto.PatientId == Guid.Empty)
            return BadRequest();

        try
        {
            await insert.Insert(dto);
            logger.LogInformation("Vaccine created successfully for patient {PatientId}", dto.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to create Vaccine for patient {PatientId}", dto.PatientId);
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID de la vacuna es obligatorio.");
        var vaccine = await get.Get(id.Value);
        if (vaccine == null)
            return NotFound("Vacuna no encontrada.");
        SetAntiforgeryToken();
        ViewBag.Description = EnumNamesToSelectList<VaccinesEnum>(name => name.Replace("_", " "));
        return PartialView("_Create", vaccine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(VaccinesDto dto)
    {
        if (dto == null || dto.Id == Guid.Empty || dto.PatientId == Guid.Empty)
            return BadRequest();

        try
        {
            await update.Update(dto);
            logger.LogInformation("Vaccine updated successfully for patient {PatientId}", dto.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to update Vaccine for patient {PatientId}", dto.PatientId);
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
            logger.LogInformation("Vaccine with ID {VaccineId} deleted successfully", id);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to delete Vaccine with ID {VaccineId}", id);
            return StatusCode(500);
        }
    }


}
