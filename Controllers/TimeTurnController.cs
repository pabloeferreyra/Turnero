using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TimeTurnController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimeTurnController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TimeTurn
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var size = 10;
            var tTurns = _context.TimeTurns.OrderBy(t => t.Time);
            return View(await PaginatedList<TimeTurnViewModel>.CreateAsync(tTurns.AsNoTracking(), pageNumber ?? 1, size));
        }

        // GET: TimeTurn/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TimeTurn/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Time")] TimeTurnViewModel timeTurnViewModel)
        {
            if (ModelState.IsValid)
            {
                timeTurnViewModel.Id = Guid.NewGuid();
                _context.Add(timeTurnViewModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(timeTurnViewModel);
        }

        // GET: TimeTurn/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeTurnViewModel = await _context.TimeTurns
                .FirstOrDefaultAsync(m => m.Id == id);
            if (timeTurnViewModel == null)
            {
                return NotFound();
            }

            return View(timeTurnViewModel);
        }

        // POST: TimeTurn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var timeTurnViewModel = await _context.TimeTurns.FindAsync(id);
            _context.TimeTurns.Remove(timeTurnViewModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TimeTurnViewModelExists(Guid id)
        {
            return _context.TimeTurns.Any(e => e.Id == id);
        }
    }
}
