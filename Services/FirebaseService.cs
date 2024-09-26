


using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Text.Json;

namespace Turnero.Services;

public class FirebaseService(HttpClient httpClient, IConfiguration configuration, UserManager<IdentityUser> userManager) : IFirebaseService
{
    public async Task<UserRecord> RegisterAsync(UserRegisterRequestDTO usrDto)
    {
        var userArgs = new UserRecordArgs { DisplayName = usrDto.Name, Email = usrDto.Email, Password = usrDto.Password };
        var user = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        var tid = await LoginAsync(new UserLoginRequestDTO { Email = usrDto.Email, Password = usrDto.Password });
        var iUser = new IdentityUser
        {
            Id = tid.LocalId,
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

        //var sent = await SendEmailVerificationLinkAsync(tid.IdToken);
        return user;
    }

    public async Task<IdentityResult> RegisterAdminAsync(UserRegisterRequestDTO usrDto)
    {
        var userArgs = new UserRecordArgs { DisplayName = usrDto.Name, Email = usrDto.Email, Password = usrDto.Password };
        await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        var tid = await LoginAsync(new UserLoginRequestDTO { Email = usrDto.Email, Password = usrDto.Password });
        var user = new IdentityUser
        {
            Id = tid.LocalId,
            UserName = usrDto.Name,
            Email = usrDto.Email
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            throw new Exception();
        }

        await userManager.AddToRoleAsync(user, usrDto.Role);
        return createResult;
    }

    public async Task<AuthFirebase> LoginAsync(UserLoginRequestDTO usrDto)
    {
        var credentials = new { 
            usrDto.Email, 
            usrDto.Password, 
            returnSecureToken = true
        };

        var response = await httpClient.PostAsJsonAsync("", credentials);
        var authFirebaseObject = await response.Content.ReadFromJsonAsync<AuthFirebase>();
        return authFirebaseObject;
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
                idToken = idToken
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
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
}