using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Turnero.Data;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Utilities;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class MedicsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IGetMedicsServices _getMedicsServices;
    private readonly IInsertMedicServices _insertMedicServices;
    private readonly IUpdateMedicServices _updateMedicServices;
    public ILogger<AdministrationController> Logger { get; }

    public MedicsController(RoleManager<IdentityRole> roleManager,
                            UserManager<IdentityUser> userManager,
                            ILogger<AdministrationController> logger,
                            ApplicationDbContext context,
                            IGetMedicsServices getMedicsServices,
                            IInsertMedicServices insertMedicServices, 
                            IUpdateMedicServices updateMedicServices)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.Logger = logger;
        _context = context;
        _getMedicsServices = getMedicsServices;
        _insertMedicServices = insertMedicServices;
        _updateMedicServices = updateMedicServices;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _getMedicsServices.GetMedics());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medic = await _getMedicsServices.GetMedicById((Guid)id);
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
            await _insertMedicServices.Create(medic);
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

        var medic = await _getMedicsServices.GetMedicById((Guid)id);
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
            bool resUpd = await _updateMedicServices.Update(medic);
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

        var medic = await _getMedicsServices.GetMedicById((Guid)id);
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
            var medic = await _getMedicsServices.GetMedicById(id);
            _updateMedicServices.Delete(medic);
        }
        return RedirectToAction(nameof(Index));
    }

    private bool MedicExists(Guid id)
    {
        return _getMedicsServices.ExistMedic(id);
    }
}
