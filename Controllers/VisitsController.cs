namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class VisitsController(IGetVisitService getVisit,
    IInsertVisitService insertVisit,
    ILogger<VisitsController> logger,
    IGetMedicsServices getMedics) : TurneroBaseController
{
    [HttpGet]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            return BadRequest("Visit ID is required.");
        }
        ViewData["VisitId"] = id.ToString();
        var Details = await getVisit.Get(id.Value);
        return PartialView("_Details", Details);
    }

    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            return BadRequest("Patient ID is required.");
        }
        ViewData["PatientId"] = id.ToString();
        // Pass a Visit model so the Razor partial has a non-null Model
        var model = new Visit { PatientId = id.Value };
        SetAntiforgeryToken();
        return PartialView("_CreateVisit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(Visit visit)
    {
        string isMedic = await CheckMedic();
        var medicId = Guid.TryParse(isMedic, out var mid) ? mid : (Guid?)null;
        if (medicId != null)
        {
            visit.MedicId = (Guid)medicId;
        }
        if (visit == null || visit.PatientId == Guid.Empty)
        {
            return BadRequest();
        }
        try
        {
            await insertVisit.Create(visit);
            logger.LogInformation("Visit created successfully for patient {PatientId}", visit.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to create visit for patient {PatientId}", visit.PatientId);
            return StatusCode(500);
        }

    }

    [HttpGet]
    public IActionResult Index(Guid id)
    {
        logger.LogInformation("GetVisits called for patient {PatientId}", id);
        ViewData["PatientId"] = id.ToString();
        return PartialView("_VisitsTable");
    }

    [HttpPost]
    public async Task<IActionResult> InitializeVisits(Guid? patientId)
    {
        logger.LogInformation("InitializeVisits called. Request path: {Path}. Query patientId: {QueryPatientId}", Request.Path, patientId?.ToString() ?? "null");
        try
        {
            var result = await DataTablesHelper.InitializePatientDataTablesAsync(
                Request, patientId,
                async pid => (await getVisit.SearchVisits(pid))?.ToList() ?? [],
                logger);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InitializeVisits failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }


}
