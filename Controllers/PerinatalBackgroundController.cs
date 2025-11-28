using System.Threading.Tasks;

namespace Turnero.Controllers;

public class PerinatalBackgroundController(IGetPerinatalBackgroundService get,
    IUpdatePerinatalBackgroundService update,
    ILogger<PerinatalBackgroundController> logger) : Controller
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        var data  = await get.Get(id.Value);
        return PartialView("_Details", data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del antecedente perinatal es obligatorio.");
        var data = await get.Get(id.Value);
        if (data == null)
            return NotFound();
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        return PartialView("_Edit", data);
    }

    [HttpPut]
    public async Task<IActionResult> Edit(PerinatalBackground data)
    {
        try
        {
            ModelState.Remove("Patient");
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await update.Update(data);
            return await Index(data.Id);
        }
        catch (Exception ex)
        {
            logger.LogError("Error in {Action}: {Message}", nameof(Edit), ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
