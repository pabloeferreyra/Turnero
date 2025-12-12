namespace Turnero.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FirebaseController(IFirebaseService firebaseService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserRecord>> Register([FromBody] UserFirebaseDTO userRegister)
    {
        var obj = await firebaseService.RegisterAsync(userRegister);
        return obj;
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<Object>> Login([FromBody] UserLoginRequestDTO request)
    {
        var obj = await firebaseService.LoginAsync(request);

        return obj;
    }
}
