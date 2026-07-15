namespace Turnero.Controllers;

public class HomeController(IGetTurnsServices getTurns,
                      IGetMedicsServices getMedics,
                      IGetTimeTurnsServices getTimeTurns,
                      IMemoryCache cache) : TurneroBaseController
{
    public async Task<IActionResult> Index()
    {
        await GetCachedMedicsAsync();
        await GetCachedTimeTurnsAsync();

        var turnsAsync = getTurns.GetTurns(DateTime.Today, null);
        List<int> turns =
        [
            turnsAsync.Where(t => t.Accessed).Count(),
            turnsAsync.Where(t => !t.Accessed).Count()
        ];

        return View(turns);
    }
}
