namespace Turnero.Web.Controllers;

public class VademecumController(IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    public IActionResult Index()
    {
        return PartialView("_Search");
    }
}
