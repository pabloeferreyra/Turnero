using Turnero.SL.Services.TurnsServices;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class TimeTurnController(IGetTimeTurnsServices getTimeTurns,
                          IInsertTimeTurnServices insertTimeTurn,
                          IDeleteTimeTurnServices deleteTimeTurn) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await getTimeTurns.GetTimeTurns());
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

        var timeTurnViewModel = await getTimeTurns.GetTimeTurn((Guid)id);
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
        var timeTurnViewModel = await getTimeTurns.GetTimeTurn(id);
        deleteTimeTurn.Delete(timeTurnViewModel);
        return RedirectToAction(nameof(Index));
    }
}
