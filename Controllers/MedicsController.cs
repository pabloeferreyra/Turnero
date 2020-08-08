using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Controllers
{
    public class MedicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;
        public ILogger<AdministrationController> Logger { get; }

        public MedicsController(RoleManager<IdentityRole> roleManager,
                                UserManager<IdentityUser> userManager,
                                ILogger<AdministrationController> logger,
            ApplicationDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.Logger = logger;
            _context = context;
        }

        // GET: Medics
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Medics.ToListAsync());
        }

        // GET: Medics/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medic = await _context.Medics
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medic == null)
            {
                return NotFound();
            }

            return View(medic);
        }

        // GET: Medics/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync()
        {
            var users = await userManager.GetUsersInRoleAsync("Medico");

            users.Insert(0, new IdentityUser { Id = string.Empty, UserName = "Seleccione..." });
            ViewBag.User = users;
            return View();
        }

        // POST: Medics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medic medic)
        {
            if (ModelState.IsValid)
            {
                medic.Id = Guid.NewGuid();
                _context.Add(medic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(medic);
        }

        // GET: Medics/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medic = await _context.Medics.FindAsync(id);
            if (medic == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index");
        }

        // POST: Medics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Medic medic)
        {
            if (medic.Id == null)
            {
                ViewBag.ErrorMessage = $"Medic with Id = {medic.Id} cannot be found";
                return View("NotFound");
            }
            if (!MedicExists(medic.Id))
            {
                ViewBag.ErrorMessage = $"Medic with Id = {medic.Id} cannot be found";
                return View("NotFound");
            }
            else

           if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.LogError($"Error editing medic {ex}");
                    return View("Error");
                }

            }
            return RedirectToAction("Index");
        }

        // GET: Medics/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = $"Medic with Id = {id} cannot be found";
                return View("NotFound");
            }

            var medic = await _context.Medics
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medic == null)
            {
                ViewBag.ErrorMessage = $"Medic with Id = {id} cannot be found";
                return View("NotFound");
            }

            return RedirectToAction("Index");
        }

        // POST: Medics/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var medic = await _context.Medics.FindAsync(id);
            _context.Medics.Remove(medic);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MedicExists(Guid id)
        {
            return _context.Medics.Any(e => e.Id == id);
        }
    }
}
