using DocumentFormat.OpenXml.Office2010.Excel;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class ParentsDataController(
    IUpdateParentsDataService updateParentsData,
    IDeleteParentsDataService deleteParentsData,
    IGetParentsDataService getParentsData,
    ILogger<ParentsDataController> logger) : TurneroBaseController
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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
        SetAntiforgeryToken();
        ViewBag.FatherBloodtype = EnumToSelectList<BloodType>(e => e.GetDisplayName());
        ViewBag.MotherBloodtype = EnumToSelectList<BloodType>(e => e.GetDisplayName());
        return PartialView("_Edit", data);
    }

    [HttpPut]
    public async Task<IActionResult> Edit(ParentsData data)
    {
        try
        {
            await updateParentsData.UpdateParentsData(data);
            var Data = await getParentsData.GetParentsData(data.Id);
            return PartialView("_Details", Data);
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
