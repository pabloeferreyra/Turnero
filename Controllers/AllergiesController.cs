namespace Turnero.Controllers;

public class AllergiesController(IInsertAllergiesServices insertAllergies,
    IUpdateAllergiesServices updateAllergies,
    IDeleteAllergiesServices deleteAllergies,
    IGetAllergiesServices getAllergies,
    ILogger<AllergiesController> logger) : TurneroBaseController
{

    [HttpGet]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("Allergy ID is required.");
        ViewData["AllergyId"] = id.ToString();
        var Details = await getAllergies.Get(id.Value);
        return PartialView("_Details", Details);
    }

    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("Patient ID is required.");
        ViewData["PatientId"] = id.ToString();
        var model = new Allergies { PatientId = id.Value };
        SetAntiforgeryToken();
        ViewBag.Occurrency = EnumToSelectList<Occurrency>();
        ViewBag.Severity = EnumToSelectList<Severity>();
        ViewBag.Type = EnumToSelectList<AllergyType>();
        return PartialView("_Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(Allergies allergy)
    {
        if (allergy == null || allergy.PatientId == Guid.Empty)
        {
            return BadRequest();
        }
        try
        {
            await insertAllergies.InsertAllergy(allergy);
            logger.LogInformation("Allergy created successfully for patient {PatientId}", allergy.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to create allergy for patient {PatientId}", allergy.PatientId);
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult Index(Guid id)
    {
        logger.LogInformation("GetAllergies called for patient {patientId}", id);
        ViewData["PatientId"] = id.ToString();
        return PartialView("_AllergiesTable");
    }

    [HttpPost]
    public async Task<IActionResult> InitializeAllergies(Guid? patientId)
    {
        logger.LogInformation("InitializeAllergies called. Request path: {Path}. Query patientId: {QueryPatientId}", Request.Path, patientId?.ToString() ?? "null");
        try
        {
            var result = await DataTablesHelper.InitializePatientDataTablesAsync(
                Request, patientId,
                async pid => (await getAllergies.GetAllergies(pid))?.ToList() ?? [],
                logger);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InitializeAllergies failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return NotFound();
        var allergy = await getAllergies.Get(id);
        if (allergy == null)
        {
            return NotFoundError("Allergy", id.ToString());
        }
        SetAntiforgeryToken();
        ViewBag.Occurrency = EnumToSelectList<Occurrency>();
        ViewBag.Severity = EnumToSelectList<Severity>();
        ViewBag.Type = EnumToSelectList<AllergyType>();
        return PartialView("_Create", allergy);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(Allergies allergy)
    {
        if (allergy == null || allergy.PatientId == Guid.Empty)
        {
            return BadRequest();
        }
        try
        {
            await updateAllergies.UpdateAllergy(allergy);
            logger.LogInformation("Allergy updated successfully for patient {PatientId}", allergy.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to update allergy for patient {PatientId}", allergy.PatientId);
            return StatusCode(500);
        }
    }

    [HttpDelete]
    public async Task<StatusCodeResult> Delete(Guid id)
    {
        var allergy = await getAllergies.Get(id);
        deleteAllergies.DeleteAllergy(allergy);
        return Ok();
    }


}
