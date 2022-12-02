using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Turnero.Services.Interfaces;

namespace Turnero.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public IGetTurnsServices _getTurns;
    public HomeController(ILogger<HomeController> logger, IGetTurnsServices getTurns)
    {
        _logger = logger;
        _getTurns = getTurns;
    }

    public async Task<IActionResult> Index()
    {
        var turnsAsync = await _getTurns.GetTurns(null, null);
        List<int> turns = new List<int>
        {
            turnsAsync.Where(t => t.Accessed).Count(),
            turnsAsync.Where(t => t.Accessed == false).Count()
        };
        return View(turns);
    }
}
