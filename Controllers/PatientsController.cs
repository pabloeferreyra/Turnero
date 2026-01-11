namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class PatientsController(IInsertPatientService insertPatient,
    IGetPatientService getPatient,
    IGetParentsDataService getParents,
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
        var json = new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data
        };
        return await Task.FromResult<IActionResult>(Ok(json));
    }

    
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Bloodtype = Enum.GetValues<BloodType>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.GetDisplayName()
            }).ToList();
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
            ViewBag.ErrorMessage = $"Patient with Id = {id} cannot be found";
            return NotFound();
        }
        var age = CalcularEdad(patient.BirthDate);
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
            ViewBag.ErrorMessage = $"Patient with Id = {id} cannot be found";
            return NotFound();
        }
        ViewBag.Bloodtype = Enum.GetValues<BloodType>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.GetDisplayName()
            }).ToList();
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

    #region Private Methods
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
        if (skip != 0)
        {
            data = [.. data.Skip(skip).Take(pageSize).ToList()];
        }
        else if (pageSize != -1)
        {
            data = [.. data.Take(pageSize).ToList()];
        }
        return data;
    }

    private static string CalcularEdad(DateTime fechaNacimiento, DateTime? fechaReferencia = null)
    {
        var hoy = fechaReferencia?.Date ?? DateTime.Today;

        if (fechaNacimiento.Date > hoy)
            throw new ArgumentException("La fecha de nacimiento no puede ser futura.");

        // AÑOS
        int años = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento.AddYears(años) > hoy)
            años--;

        if (años >= 1)
            return $"{años} año{(años > 1 ? "s" : "")}";

        // MESES
        int meses = (hoy.Year - fechaNacimiento.Year) * 12 + hoy.Month - fechaNacimiento.Month;
        if (fechaNacimiento.AddMonths(meses) > hoy)
            meses--;

        if (meses >= 1)
            return $"{meses} mes{(meses > 1 ? "es" : "")}";

        // DÍAS / SEMANAS
        int dias = (hoy - fechaNacimiento.Date).Days;

        if (dias >= 7)
        {
            int semanas = dias / 7;
            return $"{semanas} semana{(semanas > 1 ? "s" : "")}";
        }

        return $"{dias} día{(dias != 1 ? "s" : "")}";
    }

    #endregion
}
