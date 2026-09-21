namespace Turnero.SL.Services;

public class FirebaseService(
    HttpClient httpClient,
    IConfiguration configuration,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : IFirebaseService
{
    private const string RoleClaim = "role";
    private const string DefaultRole = "Medico";

    public async Task<UserRecord> RegisterAsync(UserFirebaseDTO usrDto)
    {
        await RegisterAdminAsync(usrDto);
        return await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(usrDto.Email);
    }

    public async Task<IdentityResult> RegisterAdminAsync(UserFirebaseDTO usrDto)
    {
        var userArgs = new UserRecordArgs { DisplayName = usrDto.Name, Email = usrDto.Email, Password = usrDto.Password };
        var firebaseUser = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        await SetRoleAsync(firebaseUser.Uid, DefaultRole);

        var user = new IdentityUser
        {
            Id = firebaseUser.Uid,
            UserName = usrDto.Name,
            Email = usrDto.Email
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(firebaseUser.Uid);
            return createResult;
        }

        if (!await roleManager.RoleExistsAsync(DefaultRole))
        {
            await roleManager.CreateAsync(new IdentityRole(DefaultRole));
        }

        var roleResult = await userManager.AddToRoleAsync(user, DefaultRole);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(firebaseUser.Uid);
            return roleResult;
        }

        var tid = await LoginAsync(new UserLoginRequestDTO { Email = usrDto.Email, Password = usrDto.Password });
        await SendEmailVerificationLinkAsync(tid.IdToken);
        return createResult;
    }

    public Task SetRoleAsync(string userId, string? role)
    {
        var claims = string.IsNullOrWhiteSpace(role)
            ? new Dictionary<string, object>()
            : new Dictionary<string, object> { [RoleClaim] = role };

        return FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(userId, claims);
    }

    public async Task<string?> GetRoleAsync(string userId)
    {
        var user = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);
        return user.CustomClaims is not null && user.CustomClaims.TryGetValue(RoleClaim, out var role)
            ? role?.ToString()
            : null;
    }

    public Task DeleteUserAsync(string userId)
    {
        return FirebaseAuth.DefaultInstance.DeleteUserAsync(userId);
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
    Task SetRoleAsync(string userId, string? role);
    Task<string?> GetRoleAsync(string userId);
    Task DeleteUserAsync(string userId);
    Task<AuthFirebase> LoginAsync(UserLoginRequestDTO usrDto);
    Task<HttpStatusCode> SendPasswordResetLinkAsync(string email);
    Task<HttpStatusCode> UpdatePasswordAsync(UserResetPasswordDTO userReset);
}
