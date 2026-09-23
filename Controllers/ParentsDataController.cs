using DocumentFormat.OpenXml.Office2010.Excel;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class ParentsDataController(
    IUpdateParentsDataService updateParentsData,
    IDeleteParentsDataService deleteParentsData,
    IGetParentsDataService getParentsData,
    ILogger<ParentsDataController> logger) : TurneroBaseController
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es obligatorio.");
        var Data = await getParentsData.GetParentsData(id.Value);
        return PartialView("_Details", new ParentsDataDetailsViewModel
        {
            Data = Data,
            PatientId = id.Value
        });
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
        return PartialView("_Edit", new ParentsDataEditViewModel(data)
        {
            FatherBloodtypes = BloodTypeSelectList.Create(),
            MotherBloodtypes = BloodTypeSelectList.Create()
        });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ParentsData data)
    {
        try
        {
            await updateParentsData.UpdateParentsData(data);
            var Data = await getParentsData.GetParentsData(data.Id);
            return PartialView("_Details", new ParentsDataDetailsViewModel
            {
                Data = Data,
                PatientId = data.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError("Error in {Action}: {Message}", nameof(Edit), ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] ParentsData data)
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
