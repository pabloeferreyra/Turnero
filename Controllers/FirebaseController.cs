using Microsoft.AspNetCore.Identity;

namespace Turnero.Controllers;

[Route("api/[controller]")]
[ApiController]
//[AllowAnonymous]
public class FirebaseController : ControllerBase
{
    private readonly IFirebaseService _firebaseService;
    public FirebaseController(IFirebaseService firebaseService,
        UserManager<IdentityUser> userManager)
    {
        _firebaseService = firebaseService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserRecord>> Register([FromBody]UserRegisterRequestDTO userRegister)
    {
        var obj = await _firebaseService.RegisterAsync(userRegister);
        return obj;
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<Object>> Login([FromBody] UserLoginRequestDTO request)
    {
        var obj = await _firebaseService.LoginAsync(request);

        return obj;
    }
}
