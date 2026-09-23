namespace Turnero.Application.Common.Interfaces;

/// <summary>
/// Puerto de Application: autenticación de usuarios contra Firebase Auth.
/// Los DTOs son propios de Application; la presentación no referencia
/// FirebaseAdmin ni tipos de Identity de Infrastructure.
/// </summary>
public interface IFirebaseAuthService
{
    /// <summary>
    /// Registra el usuario en Firebase + Identity local con rol por defecto y envía email de verificación.
    /// </summary>
    Task<RegistrationResultDto> RegisterUserAsync(UserFirebaseDTO user);

    /// <summary>Autentica contra Firebase y devuelve el token con sus claims.</summary>
    Task<FirebaseSessionDto> LoginAsync(UserLoginRequestDTO credentials);

    /// <summary>Establece el claim custom "role" del usuario en Firebase.</summary>
    Task SetRoleAsync(string userId, string? role);

    /// <summary>Lee el claim custom "role" del usuario en Firebase (null si no tiene).</summary>
    Task<string?> GetRoleAsync(string userId);

    /// <summary>Elimina el usuario de Firebase.</summary>
    Task DeleteUserAsync(string userId);

    /// <summary>Envía el email de reset de password. Devuelve el HTTP status del proveedor.</summary>
    Task<System.Net.HttpStatusCode> SendPasswordResetLinkAsync(string email);

    /// <summary>Cambia la contraseña usando el token de reset. Devuelve el HTTP status del proveedor.</summary>
    Task<System.Net.HttpStatusCode> UpdatePasswordAsync(UserResetPasswordDTO userReset);
}
