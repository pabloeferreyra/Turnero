using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnero.SL.Services.Interfaces.TimeTurn;
using Turnero.Utilities.Constants;

namespace Turnero.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
public class TimeTurnsApiController(IGetTimeTurnsServices getTimeTurns) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var times = await getTimeTurns.GetCachedTimes();
        return Ok(times.Select(t => new { t.Id, t.Time }));
    }
}
