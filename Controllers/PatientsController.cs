using Microsoft.JSInterop.Infrastructure;

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
        var patients = getPatient.GetAllPatients();
        _ = SetTable(patients, out string draw, out int pageSize, out int skip, out List<PatientDTO> data, out int recordsTotal);
        data = SetPage(pageSize, skip, data);
        var json = new {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data
        };
        return await Task.FromResult<IActionResult>(Ok(json));
    }

    private IQueryable<PatientDTO> SetTable(IQueryable<PatientDTO> patients, out string draw, out int pageSize, out int skip, out List<PatientDTO> data, out int recordsTotal)
    {
        draw = Request.Form["draw"].FirstOrDefault();
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var nameSearch = Request.Form["Columns[1][search][value]"].FirstOrDefault();
        var dniSearch = Request.Form["Columns[2][search][value]"].FirstOrDefault();

        pageSize = length != null ? int.Parse(length) : 0;
        skip = start != null ? int.Parse(start) : 0;

        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

        

        if (!(string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortColumnDirection)))
        {
            data = [.. patients.OrderBy(sortColumn + " " + sortColumnDirection)];
        }

        if(!string.IsNullOrEmpty(nameSearch) && string.IsNullOrEmpty(dniSearch))
        {
            data = [.. patients.Where(m => m.Name.Contains(nameSearch))];
        }
        else if(!string.IsNullOrEmpty(dniSearch) && string.IsNullOrEmpty(nameSearch))
        {
            data = [.. patients.Where(m => m.Dni.ToString().Contains(dniSearch))];
        }
        else if(!string.IsNullOrEmpty(nameSearch) && !string.IsNullOrEmpty(dniSearch))
        {
            data = [.. patients.Where(m => m.Name.Contains(nameSearch) && m.Dni.ToString().Contains(dniSearch))];
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


    public async Task<List<PatientDTO>> Search(string? Name, int? Dni)
    {
        
        string search = $"{Name} {Dni}".Trim();
        List<PatientDTO> patients = await getPatient.SearchPatients(search);
        return patients;

    }
    [HttpGet]
    public IActionResult Create()
    {
        return PartialView("_Create");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
}
