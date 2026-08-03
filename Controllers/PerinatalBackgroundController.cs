namespace Turnero.Controllers;

public class PerinatalBackgroundController(IGetPerinatalBackgroundService get,
    IUpdatePerinatalBackgroundService update,
    ILogger<PerinatalBackgroundController> logger) : TurneroBaseController
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        var data = await get.Get(id.Value);
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
        SetAntiforgeryToken();
        return PartialView("_Edit", data);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PerinatalBackground data)
    {
        try
        {
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
