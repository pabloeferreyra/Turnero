namespace Turnero.Controllers;

public class VademecumController : TurneroBaseController
{
    public IActionResult Index()
    {
        return PartialView("_Search");
    }
}
