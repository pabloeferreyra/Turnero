namespace Turnero.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FirebaseController(IFirebaseAuthService firebaseService) : ControllerBase
{
    [Authorize(Roles = RolesConstants.Admin)]
    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResultDto>> Register([FromBody] UserFirebaseDTO userRegister)
    {
        var obj = await firebaseService.RegisterUserAsync(userRegister);
        return obj;
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<FirebaseSessionDto>> Login([FromBody] UserLoginRequestDTO request)
    {
        var obj = await firebaseService.LoginAsync(request);

        return obj;
    }
}
