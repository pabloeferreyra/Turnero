namespace Turnero.SL.Services;

public class FirebaseService(HttpClient httpClient, IConfiguration configuration, UserManager<IdentityUser> userManager) : IFirebaseService
{
    public async Task<UserRecord> RegisterAsync(UserFirebaseDTO usrDto)
    {
        var userArgs = new UserRecordArgs { DisplayName = usrDto.Name, Email = usrDto.Email, Password = usrDto.Password };
        var user = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        var tid = await LoginAsync(new UserLoginRequestDTO { Email = usrDto.Email, Password = usrDto.Password });
        var iUser = new IdentityUser
        {
            Id = tid.LocalId ?? string.Empty, // Soluciona CS8601 asegurando que nunca se asigne null
            UserName = usrDto.Name,
            Email = usrDto.Email
        };

        var createResult = await userManager.CreateAsync(iUser);
        if (!createResult.Succeeded)
        {
            throw new Exception();
        }

        if (iUser.UserName == "Administrador")
            await userManager.AddToRoleAsync(iUser, "Admin");
        else if (iUser.UserName.Equals("Ingreso", StringComparison.InvariantCultureIgnoreCase) || iUser.UserName.Equals("IngresoPruebas", StringComparison.InvariantCultureIgnoreCase))
            await userManager.AddToRoleAsync(iUser, "Ingreso");
        else
            await userManager.AddToRoleAsync(iUser, "Medico");

        return user;
    }

    public async Task<IdentityResult> RegisterAdminAsync(UserFirebaseDTO usrDto)
    {
        var userArgs = new UserRecordArgs { DisplayName = usrDto.Name, Email = usrDto.Email, Password = usrDto.Password };
        await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        var tid = await LoginAsync(new UserLoginRequestDTO { Email = usrDto.Email, Password = usrDto.Password });
        var user = new IdentityUser
        {
            Id = tid.LocalId ?? string.Empty, // Soluciona CS8601 asegurando que nunca se asigne null
            UserName = usrDto.Name,
            Email = usrDto.Email
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            throw new Exception();
        }

        await userManager.AddToRoleAsync(user, usrDto.Role);
        await SendEmailVerificationLinkAsync(tid.IdToken);
        return createResult;
    }

    public async Task<AuthFirebase> LoginAsync(UserLoginRequestDTO usrDto)
    {
        var credentials = new
        {
            usrDto.Email,
            usrDto.Password,
            returnSecureToken = true
        };

        var response = await httpClient.PostAsJsonAsync("", credentials);
        var authFirebaseObject = await response.Content.ReadFromJsonAsync<AuthFirebase>();
        return authFirebaseObject ?? throw new InvalidOperationException("No se pudo obtener la autenticación de Firebase.");
    }

    private async Task<string> SendEmailVerificationLinkAsync(string idToken)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = configuration["Authentication:TokenCode"];

            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                return "Verification email sent successfully.";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return $"Error sending verification email: {errorResponse}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }

    public async Task<HttpStatusCode> SendPasswordResetLinkAsync(string email)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = configuration["Authentication:TokenCode"];

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, content);

            return response.StatusCode;
        }
        catch (Exception)
        {
            return HttpStatusCode.InternalServerError;
        }
    }

    public async Task<HttpStatusCode> UpdatePasswordAsync(UserResetPasswordDTO userReset)
    {
        try
        {
            using var client = new HttpClient();
            var idToken = await userManager.FindByEmailAsync(userReset.Email);
            var requestUri = configuration["Authentication:TokenReset"];

            var payload = new
            {
                idToken.Id,
                password = userReset.NewPassword,
                returnSecureToken = true
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, content);

            return response.StatusCode;
        }
        catch (Exception)
        {
            return HttpStatusCode.InternalServerError;
        }
    }
}

public interface IFirebaseService
{
    Task<UserRecord> RegisterAsync(UserFirebaseDTO usrDto);
    Task<IdentityResult> RegisterAdminAsync(UserFirebaseDTO usrDto);
    Task<AuthFirebase> LoginAsync(UserLoginRequestDTO usrDto);
    Task<HttpStatusCode> SendPasswordResetLinkAsync(string email);
    Task<HttpStatusCode> UpdatePasswordAsync(UserResetPasswordDTO userReset);
}
