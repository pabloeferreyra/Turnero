namespace Turnero.Web.Controllers;

public class HomeController(IGetTurnsServices getTurns,
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    public async Task<IActionResult> Index()
    {
        var medicsTask = GetCachedMedicsAsync();
        var timeTask = GetCachedTimeTurnsAsync();
        await Task.WhenAll(medicsTask, timeTask);

        var turnsAsync = getTurns.GetTurns(DateTime.Today, null);
        List<int> turns =
        [
            turnsAsync.Count(t => t.Accessed),
            turnsAsync.Count(t => !t.Accessed)
        ];

        return View(turns);
    }
}
