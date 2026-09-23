

namespace Turnero.Web.Controllers;

public class CongErrorController(IGetCongErrorService get, 
    IUpdateCongErrorService update, 
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery,
    ILogger<CongErrorController> logger) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        var data = await get.GetCongError(id.Value);
        return PartialView("_Details", data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del error congénito es obligatorio.");
        var data = await get.GetCongError(id.Value);
        ViewData["PatientId"] = id.Value.ToString();

        if (data == null)
            return NotFound();
        SetAntiforgeryToken();
        return PartialView("_Create", new CongErrorEditViewModel(data)
        {
            Results = CongErrorEditViewModel.CreateResultList()
        });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CongErrors data)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await update.UpdateCongError(data);
            return await Index(data.Id);
        }
        catch (Exception ex)
        {
            logger.LogError("Error in {Action}: {Message}", nameof(Edit), ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

}
