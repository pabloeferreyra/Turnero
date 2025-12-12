namespace Turnero.Controllers;

public class VademecumController : Controller
{
    public IActionResult Index()
    {
        return PartialView("_Search");
    }
}
