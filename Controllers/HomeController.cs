namespace Turnero.Controllers;

public class HomeController(ILogger<HomeController> logger,
                      IGetTurnsServices getTurns,
                      IGetMedicsServices getMedics,
                      IGetTimeTurnsServices getTimeTurns,
                      IMemoryCache cache) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    public IGetTurnsServices _getTurns = getTurns;
    public IGetMedicsServices _getMedics = getMedics;
    public IGetTimeTurnsServices _getTimeTurns = getTimeTurns;
    public IMemoryCache _cache = cache;

    public async Task<IActionResult> Index()
    {
        if (_cache.Get<List<MedicDto>>("medics").IsNullOrEmpty())
        {
            await _getMedics.GetCachedMedics();
        }

        if (_cache.Get<List<TimeTurn>>("timeTurns").IsNullOrEmpty())
        {
            await _getTimeTurns.GetCachedTimes();
        }

        var turnsAsync = _getTurns.GetTurns(DateTime.Today, null);
        List<int> turns =
        [
            turnsAsync.Where(t => t.Accessed).Count(),
            turnsAsync.Where(t => !t.Accessed).Count()
        ];

        return View(turns);
    }
}
