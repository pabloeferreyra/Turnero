namespace Turnero.SL.Services.Interfaces;

public interface IFirebaseService
{
    Task<UserRecord> RegisterAsync(UserFirebaseDTO usrDto);
    Task<IdentityResult> RegisterAdminAsync(UserFirebaseDTO usrDto);
    Task<AuthFirebase> LoginAsync(UserLoginRequestDTO usrDto);
    Task<HttpStatusCode> SendPasswordResetLinkAsync(string email);
    Task<HttpStatusCode> UpdatePasswordAsync(UserResetPasswordDTO userReset);
}
