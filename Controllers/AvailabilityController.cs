namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
public class AvailabilityController(UserManager<IdentityUser> userManager,
    IGetAvailableService getAvailable,
    IInsertAvailableService insertAvailable,
    IEditAvailableService editAvailable,
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IMemoryCache cache) : Controller
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    public IGetAvailableService _getAvailable = getAvailable;
    public IInsertAvailableService _insertAvailable = insertAvailable;
    public IEditAvailableService _editAvailable = editAvailable;
    public IGetMedicsServices _getMedics = getMedics;
    public IGetTimeTurnsServices _getTimeTurns = getTimeTurns;
    public IMemoryCache _cache = cache;

    [Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
    public async Task<IActionResult> Index()
    {
        List<MedicDto> medics = null;

        medics = _cache.Get<List<MedicDto>>("medics");
        if (medics.IsNullOrEmpty())
        {
            Task medicsTask = Task.Run(() =>
            {
                medics = _getMedics.GetCachedMedics().Result;
            });
            await medicsTask;
        }
        
        ViewBag.Medics = new SelectList(medics, "Id", "Name");
        
        return View(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, string name)
    {

        List<TimeTurn> time = null;
        if (time.IsNullOrEmpty())
        {
            Task timeTask = Task.Run(() =>
            {
                time = _getTimeTurns.GetCachedTimes().Result;
            });
            await timeTask;
        }
        ViewBag.MedicId = id;
        ViewBag.Name = name;
        ViewBag.Time = new SelectList(time, "Id", "Time");

        return PartialView("_Edit");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(List<Available> availables)
    {
        try
        {
            var exist = _getAvailable.GetAvailablesForMedic(availables.First().Id).Count != 0;
            if (!exist)
            {
                foreach (var available in availables)
                {
                    await _insertAvailable.InsertAsync(available);
                }
            }
            else
            {
                foreach(var available in availables)
                {
                    await _editAvailable.EditAsync(available);
                }
            }

            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }
}
