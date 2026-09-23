using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Turnero.Application.DTOs;

namespace Turnero.Infrastructure.Identity;

/// <summary>
/// Adaptador de Infrastructure: implementa IFirebaseAuthService (puerto de
/// Application) usando el SDK FirebaseAdmin y HttpClient. Traduce entre los
/// DTOs de Application y los tipos del SDK; el resto de la app no los ve.
/// </summary>
public class FirebaseAuthService(
    HttpClient httpClient,
    IConfiguration configuration,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : IFirebaseAuthService
{
    private const string RoleClaim = "role";
    private const string DefaultRole = "Medico";

    public async Task<RegistrationResultDto> RegisterUserAsync(UserFirebaseDTO usrDto)
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
            return ToResultDto(createResult);
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
            return ToResultDto(roleResult);
        }

        var tid = await LoginAsync(new UserLoginRequestDTO { Email = usrDto.Email, Password = usrDto.Password });
        await SendEmailVerificationLinkAsync(tid.IdToken!);
        return RegistrationResultDto.Success();
    }

    public async Task<FirebaseSessionDto> LoginAsync(UserLoginRequestDTO usrDto)
    {
        var credentials = new
        {
            usrDto.Email,
            usrDto.Password,
            returnSecureToken = true
        };

        var response = await httpClient.PostAsJsonAsync("", credentials);
        var authFirebaseObject = await response.Content.ReadFromJsonAsync<FirebaseSessionDto>();
        var session = authFirebaseObject
            ?? throw new InvalidOperationException("No se pudo obtener la autenticación de Firebase.");

        // Decodifica los claims custom del id token (el resto de la app no toca el SDK)
        if (!string.IsNullOrWhiteSpace(session.IdToken))
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(session.IdToken);
                session.Claims = decodedToken.Claims is not null
                    ? new Dictionary<string, object?>(decodedToken.Claims)
                    : [];
            }
            catch (Exception)
            {
                // Token inválido/expirado: se devuelven claims vacíos y el consumidor valida LocalId/IdToken.
            }
        }

        return session;
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

    public async Task<string> SendEmailVerificationLinkAsync(string idToken)
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

    private static RegistrationResultDto ToResultDto(IdentityResult result) =>
        result.Succeeded
            ? RegistrationResultDto.Success()
            : RegistrationResultDto.Failed([.. result.Errors.Select(e => new RegistrationErrorDto
            {
                Code = e.Code,
                Description = e.Description
            })]);
}
