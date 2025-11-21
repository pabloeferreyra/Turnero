namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class ParentsDataController(IInsertParentsDataService insertParentsData,
    IUpdateParentsDataService updateParentsData,
    IDeleteParentsDataService deleteParentsData,
    IGetParentsDataService getParentsData,
    ILogger<ParentsDataController> logger) : Controller
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        var Data = await getParentsData.GetParentsData(id.Value);
        return PartialView("_Details", Data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID de los datos parentales es obligatorio.");
        var data = await getParentsData.GetParentsData(id.Value);
        if (data == null)
            return NotFound();
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        ViewBag.FatherBloodtype = Enum.GetValues<BloodType>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.GetDisplayName()
            }).ToList();
        ViewBag.MotherBloodtype = Enum.GetValues<BloodType>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.GetDisplayName()
            }).ToList();
        return PartialView("_Edit", data);
    }

    [HttpPut]
    public async Task<StatusCodeResult> Edit(ParentsData data)
    {
        try
        {
            await updateParentsData.UpdateParentsData(data);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in {Action}: {Message}", nameof(Edit), ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete]
    public IActionResult Delete([FromBody] ParentsData data)
    {
        try
        {
            if (data == null)
            {
                return BadRequest();
            }
            deleteParentsData.DeleteParentsData(data);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in {Action}: {Message}", nameof(Delete), ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
