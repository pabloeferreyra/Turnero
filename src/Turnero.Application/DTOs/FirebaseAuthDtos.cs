using System.Text.Json.Serialization;

namespace Turnero.Application.DTOs;

/// <summary>
/// Resultado de un registro de usuario (equivalente portable de IdentityResult,
/// sin dependencia de ASP.NET Identity en los contratos de Application).
/// </summary>
public class RegistrationResultDto
{
    public bool Succeeded { get; init; }

    public List<RegistrationErrorDto> Errors { get; init; } = [];

    public static RegistrationResultDto Success() => new() { Succeeded = true };

    public static RegistrationResultDto Failed(params RegistrationErrorDto[] errors) =>
        new() { Succeeded = false, Errors = [.. errors] };
}

/// <summary>Error individual de un registro.</summary>
public class RegistrationErrorDto
{
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Sesión autenticada de Firebase: token id + claims decodificados.
/// La presentación consume los claims sin conocer el SDK de Firebase.
/// </summary>
public class FirebaseSessionDto
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("localId")]
    public string? LocalId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("idToken")]
    public string? IdToken { get; set; }

    [JsonPropertyName("registered")]
    public bool Registered { get; set; }

    /// <summary>Claims custom decodificados del token (ej. "role").</summary>
    public Dictionary<string, object?> Claims { get; set; } = [];
}
