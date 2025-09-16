namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class MedicsController(UserManager<IdentityUser> userManager,
                        IGetMedicsServices getMedicsServices,
                        IInsertMedicServices insertMedicServices,
                        IUpdateMedicServices updateMedicServices) : Controller
{

    public async Task<IActionResult> Index()
    {
        return View(await getMedicsServices.GetMedics());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medic = await getMedicsServices.GetMedicById((Guid)id);
        if (medic == null)
        {
            return NotFound();
        }

        return View(medic);
    }

    public async Task<IActionResult> CreateAsync()
    {
        var users = await userManager.GetUsersInRoleAsync("Medico");

        users.Insert(0, new IdentityUser { Id = string.Empty, UserName = "Seleccione..." });
        ViewBag.User = users;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medic medic)
    {
        if (ModelState.IsValid)
        {
            await insertMedicServices.Create(medic);
            return RedirectToAction(nameof(Index));
        }
        return View(medic);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medic = await getMedicsServices.GetMedicById((Guid)id);
        if (medic == null)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Medic medic)
    {
       
        if (!MedicExists(medic.Id))
        {
            ViewBag.ErrorMessage = $"Medic with Id = {medic.Id} cannot be found";
            return View("NotFound");
        }
        else if (ModelState.IsValid)
        {
            bool resUpd = await updateMedicServices.Update(medic);
            if(!resUpd)
            { 
                return View("Error");
            }
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            ViewBag.ErrorMessage = $"Medic with Id = {id} cannot be found";
            return View("NotFound");
        }

        var medic = await getMedicsServices.GetMedicById((Guid)id);
        if (medic == null)
        {
            ViewBag.ErrorMessage = $"Medic with Id = {id} cannot be found";
            return View("NotFound");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (MedicExists(id))
        {
            var medic = await getMedicsServices.GetMedicById(id);
            updateMedicServices.Delete(medic);
        }
        return RedirectToAction(nameof(Index));
    }

    private bool MedicExists(Guid id)
    {
        return getMedicsServices.ExistMedic(id);
    }
}
