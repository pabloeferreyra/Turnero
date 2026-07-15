using Turnero.SL.Services.CongErrorServices;

namespace Turnero.Controllers;

public class CongErrorController(IGetCongErrorService get, 
    IUpdateCongErrorService update, 
    ILogger<CongErrorController> logger) : TurneroBaseController
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
        ViewBag.ResultList = new SelectList(
            new[]
            {
                new { Value = CongErrorsResults.NA, Text = CongErrorsResults.NA },
                new { Value = CongErrorsResults.Normal, Text = CongErrorsResults.Normal },
                new { Value = CongErrorsResults.Patological, Text = CongErrorsResults.Patological }
            },
            "Value",
            "Text"
        );

        if (data == null)
            return NotFound();
        SetAntiforgeryToken();
        return PartialView("_Create", data);
    }

    [HttpPut]
    public async Task<IActionResult> Edit(CongErrors data)
    {
        try
        {
            ModelState.Remove("Patient");
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
