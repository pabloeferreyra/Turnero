namespace Turnero.Web.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class PersonalBackgroundController(IGetPersonalBackgroundService get,
    IUpdatePersonalBackgroundService update,
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery,
    ILogger<PersonalBackgroundController> logger) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        var data = await get.GetPersonalBackground(id.Value);
        return PartialView("_Details", data);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PersonalBackground data)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await update.UpdatePersonalBackground(data);
            return await Index(data.Id);
        }
        catch (Exception ex)
        {
            logger.LogError("Error in {Action}: {Message}", nameof(Edit), ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        SetAntiforgeryToken();
        var data = await get.GetPersonalBackground(id.Value);
        return PartialView("_Edit", data);
    }
}
