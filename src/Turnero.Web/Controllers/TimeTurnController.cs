namespace Turnero.Web.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class TimeTurnController(IGetTimeTurnsServices getTimeTurns,
                          IInsertTimeTurnServices insertTimeTurn,
                          IDeleteTimeTurnServices deleteTimeTurn,
                          IGetMedicsServices getMedics,
                          IAntiforgery antiforgery) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    // Nota: getTimeTurns se captura acá y en la base; la base lo expone como GetTimeTurns.
    private IGetTimeTurnsServices TimeTurnsServices { get; } = getTimeTurns;
    public async Task<IActionResult> Index()
    {
        return View(await TimeTurnsServices.GetTimeTurns());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Time")] TimeTurn timeTurnViewModel)
    {
        if (ModelState.IsValid)
        {
            await insertTimeTurn.Create(timeTurnViewModel);
            return RedirectToAction(nameof(Index));
        }
        return View(timeTurnViewModel);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var timeTurnViewModel = await TimeTurnsServices.GetTimeTurn((Guid)id);
        if (timeTurnViewModel == null)
        {
            return NotFound();
        }

        return View(timeTurnViewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var timeTurnViewModel = await TimeTurnsServices.GetTimeTurn(id);
        deleteTimeTurn.Delete(timeTurnViewModel);
        return RedirectToAction(nameof(Index));
    }
}
