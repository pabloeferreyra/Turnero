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

        return Redirect("/app");
    }
}
