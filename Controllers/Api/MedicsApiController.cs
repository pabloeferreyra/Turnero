using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnero.SL.Services.Interfaces.Medics;
using Turnero.Utilities.Constants;

namespace Turnero.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RolesConstants.Ingreso + ", " + RolesConstants.Medico)]
public class MedicsApiController(IGetMedicsServices getMedics) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var medics = await getMedics.GetCachedMedics();
        return Ok(medics.Select(m => new { m.Id, m.Name }));
    }
}
