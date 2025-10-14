namespace Turnero.Controllers;

public class PatientsController(UserManager<IdentityUser> userManager, 
    IInsertPatientService insertPatient,
    IGetPatientService getPatient,
    IUpdatePatientService updatePatient) : Controller
{
    public async Task<IActionResult> Index()
    {
        List<PatientDTO> patients = await getPatient.GetPatients();
        return View(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> InitializePatients()
    {
        _ = SetTable(out string draw, out int pageSize, out int skip, out List<PatientDTO> data, out int recordsTotal);
        data = SetPage(pageSize, skip, data);
        var json = new {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data
        };
        return await Task.FromResult<IActionResult>(Ok(json));
    }

    private IQueryable<PatientDTO> SetTable(out string draw, out int pageSize, out int skip, out List<PatientDTO> data, out int recordsTotal)
    {
        draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var search = Request.Form["Columns[1][search][value]"].FirstOrDefault();
        var patients = getPatient.SearchPatients(search).Result;
        pageSize = length != null ? int.Parse(length) : 0;
        skip = start != null ? int.Parse(start) : 0;

        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

        if (!(string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortColumnDirection)))
        {
            data = [.. patients.OrderBy(sortColumn + " " + sortColumnDirection)];
        }
        else
        {
            data = [.. patients];
        }
    
        recordsTotal = data.Count;
        return patients;
    }

    private static List<PatientDTO> SetPage(int pageSize, int skip, List<PatientDTO> data)
    {
        if(skip != 0)
        {
            data = [.. data.Skip(skip).Take(pageSize).ToList()];
        }
        else if(pageSize != -1)
        {
            data = [.. data.Take(pageSize).ToList()];
        }
        return data;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return PartialView("_Create");
    }

    [HttpGet]
    public IActionResult Details(Guid? id)
    {
        if (id == null)
            return NotFound();
        var patient = getPatient.GetPatientById(id.Value).Result;
        if(patient == null)
        {
            ViewBag.ErrorMessage = $"Patient with Id = {id} cannot be found";
            return NotFound();
        }
        return View("Details", patient);
    }

    [HttpPost]
    public async Task<StatusCodeResult> Create([FromBody]Patient patient)
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
        if(patient == null)
        {
            ViewBag.ErrorMessage = $"Patient with Id = {id} cannot be found";
            return NotFound();
        }
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
