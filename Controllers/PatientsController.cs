namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class PatientsController(IInsertPatientService insertPatient,
    IGetPatientService getPatient,
    IGetParentsDataService getParents,
    IUpdatePatientService updatePatient) : TurneroBaseController
{
    public async Task<IActionResult> Index()
    {
        List<PatientDTO> patients = await getPatient.GetPatients();
        return View(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> InitializePatients()
    {
        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(Request);
        var search = Request.Form["Columns[1][search][value]"].FirstOrDefault();
        var patients = await getPatient.SearchPatients(search);
        var data = patients.ToList();

        data = DataTablesHelper.ApplySorting(data, Request);
        var recordsTotal = data.Count;
        data = DataTablesHelper.ApplyPaging(data, pageSize, skip);

        return Ok(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
    }

    
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Bloodtype = EnumToSelectList<BloodType>(e => e.GetDisplayName());
        return PartialView("_Create");
    }

    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
            return NotFound();
        var patient = await getPatient.GetPatientById(id.Value);
        var parents = await getParents.GetParentsData(id.Value);
        ViewBag.ParentsData = parents;
        if (patient == null)
        {
            return NotFoundError("Patient", id.ToString());
        }
        var age = DateCalculations.CalcularEdad(patient.BirthDate);
        ViewBag.Age = age;
        return View("Details", patient);
    }

    [HttpPost]
    public async Task<StatusCodeResult> Create(Patient patient)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await insertPatient.InsertPatient(patient);
            return Ok();
        }
        catch (Exception)
        {
            return Conflict();
        }
    }
    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return NotFound();

        var patient = await getPatient.GetPatientById(id.Value);
        if (patient == null)
        {
            return NotFoundError("Patient", id.ToString());
        }
        ViewBag.Bloodtype = EnumToSelectList<BloodType>(e => e.GetDisplayName());
        return PartialView("_Edit", patient);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(Patient patient)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await updatePatient.UpdatePatient(patient);
            return Ok();
        }
        catch (Exception)
        {
            return Conflict();
        }
    }                              




}
