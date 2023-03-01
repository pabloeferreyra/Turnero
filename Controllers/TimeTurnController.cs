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
using Turnero.Services.Interfaces;

namespace Turnero.Controllers;

[Authorize(Roles = "Admin")]
public class TimeTurnController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IGetTimeTurnsServices _getTimeTurns;
    private readonly IInsertTimeTurnServices _insertTimeTurn;
    private readonly IDeleteTimeTurnServices _deleteTimeTurn;

    public TimeTurnController(ApplicationDbContext context,
                              IGetTimeTurnsServices getTimeTurns,
                              IInsertTimeTurnServices insertTimeTurn, 
                              IDeleteTimeTurnServices deleteTimeTurn)
    {
        _context = context;
        _getTimeTurns = getTimeTurns;
        _insertTimeTurn = insertTimeTurn;
        _deleteTimeTurn = deleteTimeTurn;
    }

    // GET: TimeTurn
    public async Task<IActionResult> Index()
    {
        return View(await _getTimeTurns.GetTimeTurns());
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
            await _insertTimeTurn.Create(timeTurnViewModel);
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

        var timeTurnViewModel = await _getTimeTurns.GetTimeTurn((Guid)id);
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
        var timeTurnViewModel = await _getTimeTurns.GetTimeTurn(id);
        _deleteTimeTurn.Delete(timeTurnViewModel);
        return RedirectToAction(nameof(Index));
    }
}
