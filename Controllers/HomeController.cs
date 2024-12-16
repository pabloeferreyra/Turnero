using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Turnero.DAL.Models;
using Turnero.SL.Services.Interfaces;

namespace Turnero.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public IGetTurnsServices _getTurns;
    public IGetMedicsServices _getMedics;
    public IGetTimeTurnsServices _getTimeTurns;
    public IMemoryCache _cache;
    
    public HomeController(ILogger<HomeController> logger,
                          IGetTurnsServices getTurns,
                          IGetMedicsServices getMedics,
                          IGetTimeTurnsServices getTimeTurns,
                          IMemoryCache cache)
    {
        _logger = logger;
        _getTurns = getTurns;
        _cache = cache;
        _getMedics = getMedics;
        _getTimeTurns = getTimeTurns;
    }

    public async Task<IActionResult> Index()
    {
        if (_cache.Get<List<MedicDto>>("medics") == null)
        {
            await _getMedics.GetCachedMedics();
        }

        if (_cache.Get<List<TimeTurn>>("timeTurns") == null)
        {
            await _getTimeTurns.GetCachedTimes();
        }

        var turnsAsync = _getTurns.GetTurns(DateTime.Today, null);
        List<int> turns = new()
        {
            turnsAsync.Where(t => t.Accessed).Count(),
            turnsAsync.Where(t => !t.Accessed).Count()
        };

        return View(turns);
    }
}
