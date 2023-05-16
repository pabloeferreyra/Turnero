﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Turnero.Models;
using Turnero.Services.Interfaces;

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
        List<int> turns = new() { 0, 0};

        if (_cache.Get<List<MedicDto>>("medics") == null)
        {
            Task medicsTask = Task.Run(() =>
            {
                _getMedics.GetCachedMedics();
            });
            
            await medicsTask;
        }
        if (_cache.Get<List<TimeTurnViewModel>>("timeTurns") == null)
        {
            Task timeTask = Task.Run(() =>
            {
                _getTimeTurns.GetCachedTimes();
            });

            await timeTask;
        }

        return View(turns);
    }
}
