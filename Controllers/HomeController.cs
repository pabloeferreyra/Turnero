namespace Turnero.Controllers;

public class HomeController(IGetTurnsServices getTurns,
                      IGetMedicsServices getMedics,
                      IGetTimeTurnsServices getTimeTurns,
                      IMemoryCache cache) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (cache.Get<List<MedicDto>>("medics") == null)
        {
            await getMedics.GetCachedMedics();
        }

        if (cache.Get<List<TimeTurn>>("timeTurns") == null)
        {
            await getTimeTurns.GetCachedTimes();
        }

        var turnsAsync = getTurns.GetTurns(DateTime.Today, null);
        List<int> turns =
        [
            turnsAsync.Where(t => t.Accessed).Count(),
            turnsAsync.Where(t => !t.Accessed).Count()
        ];

        return View(turns);
    }
}
