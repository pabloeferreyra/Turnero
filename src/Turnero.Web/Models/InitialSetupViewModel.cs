using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Turnero.Web.Models;

public sealed class InitialSetupViewModel : IValidatableObject
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Token de configuración")]
    public string SetupToken { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Agregue al menos un usuario.")]
    [MaxLength(25, ErrorMessage = "Puede configurar hasta 25 usuarios por vez.")]
    public List<InitialSetupUserViewModel> Users { get; set; } =
    [
        new() { Role = RolesConstants.Admin }
    ];

    [BindNever]
    public List<SelectListItem> RoleOptions { get; set; } = [];

    [BindNever]
    public string? SetupError { get; set; }

    [BindNever]
    public bool SetupComplete { get; set; }

    [BindNever]
    public string? ResultMessage { get; set; }

    [BindNever]
    public List<InitialSetupUserResult> CreatedUsers { get; set; } = [];

    [BindNever]
    public List<string> UserErrors { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Users is null || Users.Count == 0)
        {
            yield return new ValidationResult("Agregue al menos un usuario.", [nameof(Users)]);
            yield break;
        }

        if (!Users.Any(user => user.Role == RolesConstants.Admin))
        {
            yield return new ValidationResult(
                "Debe crear al menos un usuario con el rol Admin.",
                [nameof(Users)]);
        }

        var duplicateEmail = Users
            .Where(user => !string.IsNullOrWhiteSpace(user.Email))
            .GroupBy(user => user.Email!.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateEmail is not null)
        {
            yield return new ValidationResult(
                $"El correo {duplicateEmail.Key} está repetido.",
                [nameof(Users)]);
        }

        var roles = new[] { RolesConstants.Admin, RolesConstants.Medico, RolesConstants.Ingreso };
        if (Users.Any(user => !roles.Contains(user.Role, StringComparer.Ordinal)))
        {
            yield return new ValidationResult(
                "Seleccione un rol válido para cada usuario.",
                [nameof(Users)]);
        }
    }
}

public sealed class InitialSetupUserViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Rol")]
    public string Role { get; set; } = RolesConstants.Medico;
}

public sealed record InitialSetupUserResult(string Name, string Email, string Role);
