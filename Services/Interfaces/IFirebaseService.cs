namespace Turnero.Services.Interfaces;

public interface IFirebaseService
{
    Task<UserRecord> RegisterAsync(UserRegisterRequestDTO usrDto);
    Task<IdentityResult> RegisterAdminAsync(UserRegisterRequestDTO usrDto);
    Task<AuthFirebase> LoginAsync(UserLoginRequestDTO usrDto);
}
